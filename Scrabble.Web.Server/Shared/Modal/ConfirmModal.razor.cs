using System;
using System.Threading.Tasks;

namespace Scrabble.Web.Server.Shared.Modal
{
    public partial class ConfirmModal: IModalTemplate, IDisposable
    {
        public string Class => "modal--confirm";

        public void Dispose()
        {
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
        }

        protected void OnOk()
        {
            Context.Close(Result.Ok);
        }

        protected void OnCancel()
        {
            Context.Close(Result.Cancel);
        }

        public class Input 
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public string OkButtonCaption { get; set; } = "Ok";
            public string CancelButtonCaption { get; set; } = "Cancel";
        }

        public enum Result
        {
            Cancel = 0,
            Ok,
        }
    }
}
