(function () {
    window.Modal = {
        "SetModalActive": (isActive) => {
            if (isActive) {
                document.body.classList.add("modal-active-root");
            }
            else {
                document.body.classList.remove("modal-active-root");
            }
        }
    };
})();
