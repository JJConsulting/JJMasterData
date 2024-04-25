class Localization {
    private static strings: { [key: string]: string } = {};

    static initialize() {
        const lang = document.documentElement.lang;
        switch (lang) {
            case "pt-br":
                Localization.strings = {
                    Yes: "Sim",
                    No: "Não",
                    Close: "Fechar"
                };
            break;
        }
    }

    static get(key: string): string {
        return Localization.strings[key] || key;
    }
}

