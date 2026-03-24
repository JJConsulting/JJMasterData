class Localization {
    private static strings: Record<string, string> = {};

    static initialize() {
        const lang = document.documentElement.lang.toLowerCase().substring(0, 2);

        switch (lang) {
            case "pt":
                Localization.strings = {
                    Yes: "Sim",
                    No: "Não",
                    Close: "Fechar"
                };
                break;
            case "es":
                Localization.strings = {
                    Yes: "Sí",
                    No: "No",
                    Close: "Cerrar"
                };
                break;
            default:
                Localization.strings = {
                    Yes: "Yes",
                    No: "No",
                    Close: "Close"
                };
                break;
        }
    }

    static get(key: string): string {
        return Localization.strings[key] ?? key;
    }
}