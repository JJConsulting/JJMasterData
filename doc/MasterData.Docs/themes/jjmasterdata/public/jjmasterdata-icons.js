function observeAndApply(container, selector, apply) {
  if (!container) {
    return
  }

  const tryApply = () => {
    const matches = container.querySelectorAll(selector)
    if (matches.length > 0) {
      apply(matches)
      return true
    }
    return false
  }

  if (tryApply()) {
    return
  }

  const observer = new MutationObserver(() => {
    if (tryApply()) {
      observer.disconnect()
    }
  })

  observer.observe(container, { childList: true, subtree: true })
}

async function fetchItems(metaName, fallback) {
  const rel = document.querySelector(`meta[name="${metaName}"]`)?.getAttribute("content") || fallback
  const url = new URL(rel.replace(/\.html$/i, ".json"), window.location.href)
  const response = await fetch(url)
  if (!response.ok) {
    return []
  }
  const data = await response.json()
  return data.items || []
}

function prependIcon(link, iconClass, iconTokenClass) {
  if (!iconClass || link.querySelector(`.${iconTokenClass}`)) {
    return
  }

  const icon = document.createElement("i")
  icon.className = `${iconTokenClass} ${iconClass}`
  icon.setAttribute("aria-hidden", "true")
  link.prepend(icon)
}

function flattenItems(items) {
  const flattened = []

  for (const item of items) {
    flattened.push(item)
    if (Array.isArray(item.items)) {
      flattened.push(...flattenItems(item.items))
    }
  }

  return flattened
}

function findFirstHref(item) {
  if (item?.href) {
    return item.href
  }

  for (const child of item?.items || []) {
    const href = findFirstHref(child)
    if (href) {
      return href
    }
  }

  return null
}

function isUndefinedUrl(link) {
  try {
    return new URL(link.href).pathname.endsWith("/undefined")
  } catch {
    return false
  }
}

async function renderSidebarIcons() {
    try {
        const items = await fetchItems("docfx:tocrel", "toc.html")
        const iconMap = new Map(flattenItems(items).filter(item => item?.name && item?.icon).map(item => [item.name, item.icon]))
        const toc = document.getElementById("toc")

        observeAndApply(toc, "nav#toc a", links => {
            for (const link of links) {
                const icon = iconMap.get(link.textContent?.trim());
                prependIcon(link,icon, "jj-toc-icon")
            }
        })
    } catch (e){
        console.error(e)
    }
}

async function renderTopNavIcons() {
  try {
    const items = await fetchItems("docfx:navrel", "toc.html")
    const navRel = document.querySelector('meta[name="docfx:navrel"]')?.getAttribute("content") || "toc.html"
    const navUrl = new URL(navRel.replace(/\.html$/i, ".json"), window.location.href)
    const iconMap = new Map(flattenItems(items).filter(item => item?.name && item?.icon).map(item => [item.name, item.icon]))
    const linkMap = new Map(flattenItems(items)
      .filter(item => item?.name)
      .map(item => [item.name, findFirstHref(item)])
      .filter(([, href]) => href))
    const navbar = document.getElementById("navbar")

    observeAndApply(navbar, "a", links => {
      for (const link of links) {
        const label = link.textContent?.trim()
        if (isUndefinedUrl(link)) {
          const href = linkMap.get(label)
          if (href) {
            link.href = new URL(href, navUrl).href
          }
        }

        if (link.matches(".navbar-nav > .nav-item > .nav-link")) {
          prependIcon(link, iconMap.get(label), "jj-nav-icon")
        }
      }
    })
  } catch {
    // Ignore icon enhancement failures and keep the default nav.
  }
}

void renderSidebarIcons()
void renderTopNavIcons()
