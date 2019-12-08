using System;
using System.Threading;
using Microsoft.AspNetCore.Components;

namespace Scrabble.Web.Server.Shared.Modal
{
    public partial class ModalTemplate<TModel, TResult> : ComponentBase, IModalTemplate
    {
        public class ModalContext
        {
            private readonly Action<TResult> _onClose;
            private readonly Action _onStateHasChanged;

            public bool IsCancelled { get; private set; }

            public TModel Model { get; set; }
            public TResult Result { get; set; }

            public ModalContext(TModel model, TResult initialResult, Action<TResult> onClose, Action onStateHasChanged, CancellationToken cancellationToken)
            {
                _onClose = onClose;
                _onStateHasChanged = onStateHasChanged;
                Model = model;
                Result = initialResult;
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