class PopoverHelper {

    static dispose(selectorPrefix) {
        const popoverList = [].slice.call(
            document.querySelectorAll(selectorPrefix + ' [data-bs-toggle="popover"]')
        );

        popoverList.map(function (el) {
            const popover = bootstrap.Popover.getOrCreateInstance(el);
            popover.dispose();
        });
    }

    static listen(selectorPrefix) {
        const popoverList = document.querySelectorAll(
            selectorPrefix + ' [data-bs-toggle="popover"]'
        );

        popoverList.forEach(el =>
            new bootstrap.Popover(el, {
                trigger: 'focus', // popover geralmente é click
                html: true        // opcional (se usar conteúdo HTML)
            })
        );
    }

    static createPopover(id) {
        document.addEventListener("DOMContentLoaded", function () {
            new bootstrap.Popover(document.getElementById(id), {
                trigger: 'focus',
                html: true
            });
        });
    }
}