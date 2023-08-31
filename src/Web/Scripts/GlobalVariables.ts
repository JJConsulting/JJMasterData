var showWaitOnPost = true;

var bootstrapVersion = (() => {
    const htmlElement = document.querySelector('html');
    const versionAttribute = htmlElement?.getAttribute('data-bs-version');

    if (versionAttribute) {
        return parseInt(versionAttribute, 10);
    }

    return 5;
})();
const locale = document.documentElement.lang ?? 'pt-BR';
const localeCode = locale.split("-")[0] ?? 'pt';