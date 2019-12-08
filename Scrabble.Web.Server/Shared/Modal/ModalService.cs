using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Scrabble.Web.Server.Shared.Modal
{
    public class ModalService
    {
        public class ActiveModal
        {
            public Type Type { get; set; }
            public IModalTemplate Template { get; set; }
            public RenderFragment ModalBody { get; set; }
        }

        public class ModalEventArgs : EventArgs
        {
            public ActiveModal Modal { get; set; }
        }

        public event EventHandler<ModalEventArgs> OnModalShow;
        public event EventHandler<ModalEventArgs> OnModalClose;
        public event EventHandler OnModalStateHasChanged;

        public List<ActiveModal> ActiveModals = new List<ActiveModal>();

        public ActiveModal CurrentModal => ActiveModals.LastOrDefault();

        public ModalService()
        {
        }

        /// <summary>
        /// Execute a modal dialog, and wait for it to close.
        /// </summary>
        public Task<TResult> ExecuteModalTemplateAsync<TModel, TResult>(ModalTemplate<TModel, TResult> template, TModel model, TResult initialResult, CancellationToken cancellationToken = default)
        {

            if (IsModalActive(template))
            {
                throw new Exception("Modal already active");
            }

            var completionSource = new TaskCompletionSource<TResult>();

            var obj = new object();

            var context = new ModalTemplate<TModel, TResult>.ModalContext(model, initialResult, result =>
            {
                //notify modal container
                var modal = ActiveModals.FirstOrDefault(m => m.Template == template);
                ActiveModals.Remove(modal);
                OnModalClose?.Invoke(template, new ModalEventArgs
                {
                    Modal = modal
                });


                if (completionSource.Task.IsCompleted)
                {
                    throw new Exception("Trying to close modal twice!");
                }

                //complete task
                completionSource.SetResult(result);
            }, ModalStateHasChanged, cancellationToken);
             
            //render fragment;
            var newModal = new ActiveModal()
            {
                Type = null,
                Template = template,
                ModalBody = template.ChildContent(context),
            };
            ActiveModals.Add(newModal);

            //notify modal container
            OnModalShow?.Invoke(template, new ModalEventArgs
            {
                Modal = newModal,
            });

            //return task, which completes when the modal closes
            return completionSource.Task;
        }

        public Task<TResult> ExecuteModalComponentAsync<TComponent, TModel, TResult>(TModel model, CancellationToken cancellationToken = default) where TComponent : ModalComponentBase<TComponent, TModel, TResult>
        {
            var modalComponent = typeof(TComponent);
            var completionSource = new TaskCompletionSource<TResult>();

            var obj = new object();

            var context = new ModalComponentBase<TComponent, TModel, TResult>.ModalContext(result =>
            {
                //notify modal container
                var modal = ActiveModals.FirstOrDefault(m => m.Type == modalComponent);
                ActiveModals.Remove(modal);
                OnModalClose?.Invoke(modalComponent, new ModalEventArgs
                {
                    Modal = modal
                });


                if (completionSource.Task.IsCompleted)
                {
                    throw new Exception("Trying to close modal twice!");
                }

                //complete task
                completionSource.SetResult(result);
            }, ModalStateHasChanged, cancellationToken);

            //render fragment;
            ActiveModal newModal = new ActiveModal()
            {
                Type = modalComponent,
                Template = null,
            };
            newModal.ModalBody = (builder) =>
                {
                    builder.OpenComponent<TComponent>(0);
                    builder.AddAttribute(1, "Context", context);
                    builder.AddAttribute(2, "Model", model);
                    builder.AddComponentReferenceCapture(3, value =>
                    {
                        var template = value as IModalTemplate;
                        if (template != newModal.Template)
                        {
                            newModal.Template = value as IModalTemplate;

                            //redraw modal container in order to get css class from modal template instance
                            ModalStateHasChanged();
                        }
                    });
                    builder.CloseComponent();
            };
            ActiveModals.Add(newModal);

            //notify modal container
            OnModalShow?.Invoke(modalComponent, new ModalEventArgs
            {
                Modal = newModal,
            });

            //return task, which completes when the modal closes
            return completionSource.Task;
        }


        public void ModalStateHasChanged()
        {
            OnModalStateHasChanged?.Invoke(this, new EventArgs()
            {
            });
        }

        public bool IsModalActive(IModalTemplate modalTemplate)
        {
            return ActiveModals.Any(m => m.Template == modalTemplate);
        }
    }
}
