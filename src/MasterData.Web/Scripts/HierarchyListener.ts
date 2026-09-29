interface HierarchyItemResult {
    id: string;
    description?: string;
    parentId?: string;
    canExpand: boolean;
    iconCssClass?: string;
    iconColor?: string;
}

class HierarchyListener {
    static listen(selectorPrefix = "") {
        document.querySelectorAll<HTMLElement>(`${selectorPrefix}.jj-hierarchy`).forEach(hierarchy => {
            if (hierarchy.dataset.listenerAttached === "true" || hierarchy.dataset.interactive !== "true")
                return;

            hierarchy.dataset.listenerAttached = "true";
            hierarchy.addEventListener("click", event => this.handleClick(hierarchy, event));
            hierarchy.addEventListener("keydown", event => this.handleKeydown(hierarchy, event));
        });
    }

    private static handleClick(hierarchy: HTMLElement, event: Event) {
        const target = event.target as HTMLElement;
        if (target.closest(".jj-hierarchy-input, .jj-hierarchy-open")) {
            this.togglePicker(hierarchy);
            return;
        }

        const toggle = target.closest<HTMLButtonElement>(".jj-hierarchy-toggle");
        if (toggle) {
            void this.toggleNode(hierarchy, toggle.closest<HTMLElement>(".jj-hierarchy-node"));
            return;
        }

        const label = target.closest<HTMLButtonElement>(".jj-hierarchy-label");
        if (label) {
            this.selectNode(hierarchy, label.closest<HTMLElement>(".jj-hierarchy-node"));
            this.closePicker(hierarchy);
            return;
        }

        if (target.closest(".jj-hierarchy-clear")) {
            this.setSelection(hierarchy, null);
            this.closePicker(hierarchy);
            return;
        }

        const back = target.closest<HTMLButtonElement>(".jj-hierarchy-back");
        if (back && back.dataset.parentId) {
            this.selectNode(hierarchy, this.findNode(hierarchy, back.dataset.parentId));
            this.closePicker(hierarchy);
        }
    }

    private static async toggleNode(hierarchy: HTMLElement, node: HTMLElement) {
        if (!node || node.dataset.canExpand !== "true" || node.dataset.loading === "true")
            return;

        const toggle = node.querySelector<HTMLButtonElement>(":scope > .jj-hierarchy-row .jj-hierarchy-toggle");
        const existingGroup = node.querySelector<HTMLElement>(":scope > .jj-hierarchy-group");
        const expanded = node.getAttribute("aria-expanded") === "true";
        if (existingGroup) {
            existingGroup.hidden = expanded;
            this.setExpanded(hierarchy, node, toggle, !expanded);
            return;
        }

        node.dataset.loading = "true";
        if (toggle)
            toggle.disabled = true;
        try {
            const items = await this.loadChildren(hierarchy, node.dataset.id || "");
            if (items.length === 0) {
                this.markAsLeaf(node, toggle);
                return;
            }
            const group = document.createElement("ul");
            group.className = "jj-hierarchy-group";
            group.setAttribute("role", "group");
            items.forEach(item => group.append(this.createNode(hierarchy, item)));
            node.append(group);
            this.setExpanded(hierarchy, node, toggle, true);
        } catch (error) {
            console.error(error);
        } finally {
            delete node.dataset.loading;
            if (toggle)
                toggle.disabled = false;
        }
    }

    private static async loadChildren(hierarchy: HTMLElement, parentId: string): Promise<HierarchyItemResult[]> {
        const builder = new UrlBuilder();
        (hierarchy.dataset.queryString || "").split("&").forEach(pair => {
            const separator = pair.indexOf("=");
            if (separator > 0)
                builder.addQueryParameter(pair.substring(0, separator), decodeURIComponent(pair.substring(separator + 1)));
        });
        builder.addQueryParameter("parentId", parentId);

        const response = await fetch(builder.build(), getRequestOptions());
        if (!response.ok)
            throw new Error(`Unable to load hierarchy items (${response.status}).`);
        return await response.json();
    }

    private static createNode(hierarchy: HTMLElement, item: HierarchyItemResult): HTMLLIElement {
        const node = document.createElement("li");
        node.className = "jj-hierarchy-node";
        node.setAttribute("role", "treeitem");
        node.setAttribute("aria-selected", "false");
        node.dataset.id = item.id;
        node.dataset.parentId = item.parentId || "";
        node.dataset.canExpand = item.canExpand ? "true" : "false";
        if (item.canExpand)
            node.setAttribute("aria-expanded", "false");

        const row = document.createElement("div");
        row.className = "jj-hierarchy-row";
        if (item.canExpand) {
            const toggle = document.createElement("button");
            toggle.type = "button";
            toggle.className = "jj-hierarchy-toggle btn btn-link";
            toggle.setAttribute("aria-label", hierarchy.dataset.expandLabel || "Expand");
            const chevron = document.createElement("span");
            chevron.className = "fa-solid fa-chevron-right";
            chevron.setAttribute("aria-hidden", "true");
            toggle.append(chevron);
            row.append(toggle);
        } else {
            const spacer = document.createElement("span");
            spacer.className = "jj-hierarchy-toggle-spacer";
            row.append(spacer);
        }

        const label = document.createElement("button");
        label.type = "button";
        label.className = "jj-hierarchy-label btn btn-link";
        if (item.iconCssClass) {
            const icon = document.createElement("span");
            icon.className = `fa ${item.iconCssClass}`;
            if (item.iconColor)
                icon.style.color = item.iconColor;
            label.append(icon);
        }
        const text = document.createElement("span");
        text.className = "jj-hierarchy-text";
        text.textContent = item.description || item.id;
        label.append(text);
        row.append(label);
        node.append(row);
        return node;
    }

    private static markAsLeaf(node: HTMLElement, toggle: HTMLButtonElement | null) {
        node.dataset.canExpand = "false";
        node.removeAttribute("aria-expanded");
        if (!toggle)
            return;

        const spacer = document.createElement("span");
        spacer.className = "jj-hierarchy-toggle-spacer";
        toggle.replaceWith(spacer);
    }

    private static selectNode(hierarchy: HTMLElement, node: HTMLElement | null) {
        if (!node)
            return;
        this.setSelection(hierarchy, node);
    }

    private static setSelection(hierarchy: HTMLElement, node: HTMLElement | null) {
        hierarchy.querySelectorAll<HTMLElement>(".jj-hierarchy-node[aria-selected=true]").forEach(current => {
            current.setAttribute("aria-selected", "false");
            current.querySelector(":scope > .jj-hierarchy-row .jj-hierarchy-label")?.classList.remove("active");
        });

        const valueInput = document.getElementById(hierarchy.dataset.valueInputId || "") as HTMLInputElement;
        const descriptionInput = document.getElementById(hierarchy.dataset.descriptionInputId || "") as HTMLInputElement;
        if (!valueInput || !descriptionInput)
            return;
        valueInput.value = node?.dataset.id || "";
        descriptionInput.value = node?.querySelector<HTMLElement>(":scope > .jj-hierarchy-row .jj-hierarchy-text")
            ?.textContent?.trim() || "";

        if (node) {
            node.setAttribute("aria-selected", "true");
            node.querySelector(":scope > .jj-hierarchy-row .jj-hierarchy-label")?.classList.add("active");
        }

        const back = hierarchy.querySelector<HTMLButtonElement>(".jj-hierarchy-back");
        if (back) {
            back.dataset.parentId = node?.dataset.parentId || "";
            back.disabled = !back.dataset.parentId;
        }
        const clear = hierarchy.querySelector<HTMLButtonElement>(".jj-hierarchy-clear");
        if (clear)
            clear.disabled = !node;

        valueInput.dispatchEvent(new Event("change", {bubbles: true}));
    }

    private static setExpanded(hierarchy: HTMLElement, node: HTMLElement, toggle: HTMLButtonElement | null, expanded: boolean) {
        node.setAttribute("aria-expanded", expanded ? "true" : "false");
        const icon = toggle?.querySelector("span");
        icon?.classList.toggle("fa-chevron-right", !expanded);
        icon?.classList.toggle("fa-chevron-down", expanded);
        toggle?.setAttribute("aria-label", expanded
            ? hierarchy.dataset.collapseLabel || "Collapse"
            : hierarchy.dataset.expandLabel || "Expand");
    }

    private static findNode(hierarchy: HTMLElement, id: string): HTMLElement | null {
        return Array.from(hierarchy.querySelectorAll<HTMLElement>(".jj-hierarchy-node"))
            .find(node => node.dataset.id === id) || null;
    }

    private static handleKeydown(hierarchy: HTMLElement, event: KeyboardEvent) {
        const input = (event.target as HTMLElement).closest<HTMLInputElement>(".jj-hierarchy-input");
        if (input && (event.key === "Enter" || event.key === " ")) {
            event.preventDefault();
            this.togglePicker(hierarchy);
            return;
        }

        if (event.key === "Escape") {
            const panel = document.getElementById(hierarchy.dataset.panelId || "");
            if (panel && !panel.hidden) {
                event.preventDefault();
                this.closePicker(hierarchy, true);
                return;
            }
        }

        const label = (event.target as HTMLElement).closest<HTMLButtonElement>(".jj-hierarchy-label");
        if (!label)
            return;

        const labels = Array.from(hierarchy.querySelectorAll<HTMLButtonElement>(".jj-hierarchy-label"))
            .filter(candidate => candidate.offsetParent !== null);
        const index = labels.indexOf(label);
        if (event.key === "ArrowDown" && index < labels.length - 1) {
            event.preventDefault();
            labels[index + 1].focus();
        } else if (event.key === "ArrowUp" && index > 0) {
            event.preventDefault();
            labels[index - 1].focus();
        } else if (event.key === "ArrowRight") {
            event.preventDefault();
            const node = label.closest<HTMLElement>(".jj-hierarchy-node");
            if (node?.getAttribute("aria-expanded") !== "true")
                void this.toggleNode(hierarchy, node);
        } else if (event.key === "ArrowLeft") {
            const node = label.closest<HTMLElement>(".jj-hierarchy-node");
            if (node?.getAttribute("aria-expanded") === "true") {
                event.preventDefault();
                void this.toggleNode(hierarchy, node);
            } else if (node?.dataset.parentId) {
                event.preventDefault();
                this.findNode(hierarchy, node.dataset.parentId)
                    ?.querySelector<HTMLButtonElement>(":scope > .jj-hierarchy-row .jj-hierarchy-label")?.focus();
            }
        }
    }

    private static togglePicker(hierarchy: HTMLElement) {
        const panel = document.getElementById(hierarchy.dataset.panelId || "");
        if (!panel)
            return;

        const open = panel.hidden;
        this.setPickerOpen(hierarchy, panel, open);
        if (open) {
            const selectedLabel = panel.querySelector<HTMLButtonElement>(".jj-hierarchy-node[aria-selected=true] > .jj-hierarchy-row .jj-hierarchy-label");
            const firstLabel = panel.querySelector<HTMLButtonElement>(".jj-hierarchy-label");
            (selectedLabel || firstLabel)?.focus();
        }
    }

    private static closePicker(hierarchy: HTMLElement, restoreFocus = false) {
        const panel = document.getElementById(hierarchy.dataset.panelId || "");
        if (!panel)
            return;

        this.setPickerOpen(hierarchy, panel, false);
        if (restoreFocus)
            hierarchy.querySelector<HTMLElement>(".jj-hierarchy-input")?.focus();
    }

    private static setPickerOpen(hierarchy: HTMLElement, panel: HTMLElement, open: boolean) {
        panel.hidden = !open;
        hierarchy.querySelectorAll<HTMLElement>(".jj-hierarchy-open, .jj-hierarchy-input").forEach(trigger => {
            trigger.setAttribute("aria-expanded", String(open));
            trigger.classList.toggle("active", open);
        });
    }
}
