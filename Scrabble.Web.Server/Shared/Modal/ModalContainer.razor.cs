using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Scrabble.Web.Server.Shared.Modal
{
    public partial class ModalContainer : ComponentBase, IDisposable
    {
        [Inject] private ModalService ModalService { get; set; }
        [Inject] private IJSRuntime JSRuntime { get; set; }

        public List<ModalService.ActiveModal> CurrentModals => ModalService.ActiveModals;

        public bool IsModalActive => ModalService.CurrentModal != null;

        public void Dispose()
        {
            ModalService.OnModalShow -= ModalService_OnModalShow;
            ModalService.OnModalClose -= ModalService_OnModalClose;
            ModalService.OnModalStateHasChanged -= ModalService_OnModalStateHasChanged;
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            ModalService.OnModalShow += ModalService_OnModalShow;
            ModalService.OnModalClose += ModalService_OnModalClose;
            ModalService.OnModalStateHasChanged += ModalService_OnModalStateHasChanged;
        }

        private async void ModalService_OnModalShow(object sender, ModalService.ModalEventArgs e)
        {
            if (ModalService.ActiveModals.Count == 1)
            {
                await JSRuntime.InvokeAsync<object>("Modal.SetModalActive", true);
            }

            StateHasChanged();
        }

        private async void ModalService_OnModalClose(object sender, ModalService.ModalEventArgs e)
        {
            if (!IsModalActive)
            {
                await JSRuntime.InvokeAsync<object>("Modal.SetModalActive", false);
            }

            StateHasChanged();
        }

        private async void ModalService_OnModalStateHasChanged(object sender, EventArgs e)
        {
            StateHasChanged();
        }
    }
}