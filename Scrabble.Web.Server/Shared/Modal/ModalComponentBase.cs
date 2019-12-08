using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Scrabble.Web.Server.Shared.Modal
{
    public class ModalComponentBase<TComponent, TModel, TResult> : ComponentBase where TComponent: ModalComponentBase<TComponent, TModel, TResult>
    {
        [Parameter] public TModel Model { get; set; }
        [Parameter] public ModalContext Context { get; set; }

        public static Task<TResult> ExecuteModalAsync(ModalService modalService, TModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            return modalService.ExecuteModalComponentAsync<TComponent, TModel, TResult>(model, cancellationToken);
        }

        public class ModalContext
        {
            private readonly Action<TResult> _onClose;
            private readonly Action _onStateHasChanged;

            public bool IsCancelled { get; private set; }

            public ModalContext(Action<TResult> onClose, Action onStateHasChanged, CancellationToken cancellationToken)
            {
                _onClose = onClose;
                _onStateHasChanged = onStateHasChanged;
                IsCancelled = false;

                cancellationToken.Register(() =>
                {
                    IsCancelled = true;
                    _onClose(default);
                });
            }

            public void Close(TResult result)
            {
                _onClose(result);
            }

            public void StateHasChanged()
            {
                _onStateHasChanged();
            }
        }
    }
}