/**
 * Genre Recommendations – Jellyfin Web Plugin
 * Injiziert wöchentliche Genre-Empfehlungsreihen auf der Startseite,
 * VOR der "Weiterschauen"-Sektion.
 */
(function () {
    'use strict';

    var CACHE = null;
    var INJECT_TIMER = null;
    var LAST_INJECTED_HASH = null;

    function waitForApiClient(fn) {
        if (typeof ApiClient !== 'undefined') { fn(); }
        else { setTimeout(function () { waitForApiClient(fn); }, 150); }
    }

    function init() {
        // Jellyfin SPA-Navigation abhören
        document.addEventListener('viewshow', onViewShow);

        // Falls die Seite direkt auf der Startseite geladen wird
        scheduleInject(600);
    }

    function onViewShow() {
        var hash = window.location.hash || '';
        if (isHomePage(hash)) {
            CACHE = null; // Cache bei Navigation leeren damit frische Daten geladen werden
            scheduleInject(700);
        }
    }

    function isHomePage(hash) {
        return hash === '' ||
               hash === '#/' ||
               hash.indexOf('home') !== -1 ||
               hash.indexOf('index') !== -1;
    }

    function scheduleInject(delay) {
        if (INJECT_TIMER) clearTimeout(INJECT_TIMER);
        INJECT_TIMER = setTimeout(tryInject, delay || 700);
    }

    async function tryInject() {
        var currentHash = window.location.hash;

        // Prüfen ob wir auf der Startseite sind
        if (!isHomePage(currentHash)) return;

        // Bereits injizierte Reihen entfernen (bei Re-Render)
        removeInjected();

        try {
            if (!CACHE) {
                CACHE = await fetchSections();
            }

            if (!CACHE || !CACHE.genres || !CACHE.genres.length) return;

            var injected = inject(CACHE);

            if (!injected) {
                // Container noch nicht fertig – nochmal versuchen
                scheduleInject(600);
            } else {
                LAST_INJECTED_HASH = currentHash;
            }
        } catch (err) {
            console.error('[GenreRec] Fehler:', err);
        }
    }

    async function fetchSections() {
        var url = ApiClient.getUrl('GenreRecommendations/Sections');
        return await ApiClient.getJSON(url);
    }

    function removeInjected() {
        document.querySelectorAll('.gr-genre-section').forEach(function (el) {
            el.parentNode && el.parentNode.removeChild(el);
        });
    }

    function inject(data) {
        // Verschiedene Container-Selektoren für unterschiedliche Jellyfin-Versionen
        var container =
            document.querySelector('.homeSectionsContainer') ||
            document.querySelector('.sections') ||
            document.querySelector('[data-type="home"] .padded-top-focusscale') ||
            document.querySelector('.mainAnimatedPage .padded-top-focusscale');

        if (!container) return false;

        // "Weiterschauen"-Sektion finden (vor der wir einfügen wollen)
        var insertBefore = findContinueWatchingSection(container);

        var serverId = ApiClient.serverId();

        // Rückwärts einfügen damit die Reihenfolge stimmt
        var reversed = data.genres.slice().reverse();
        reversed.forEach(function (genre) {
            if (!genre.items || !genre.items.length) return;
            var section = buildSection(genre, serverId);
            if (insertBefore) {
                container.insertBefore(section, insertBefore);
            } else {
                // Fallback: an den Anfang
                container.insertBefore(section, container.firstChild);
            }
        });

        return true;
    }

    function findContinueWatchingSection(container) {
        // Suche nach der "Weiterschauen"-Sektion anhand von Überschrift oder Attributen
        var sections = container.querySelectorAll('.homeSection, .verticalSection, [data-section]');
        for (var i = 0; i < sections.length; i++) {
            var el = sections[i];
            var text = el.querySelector('h2, h3, .sectionTitle');
            if (text) {
                var t = text.textContent.toLowerCase();
                if (t.indexOf('weiterschauen') !== -1 ||
                    t.indexOf('continue') !== -1 ||
                    t.indexOf('resume') !== -1) {
                    return el;
                }
            }
            var attr = (el.getAttribute('data-section') || '').toLowerCase();
            if (attr.indexOf('continue') !== -1 || attr.indexOf('resume') !== -1) {
                return el;
            }
        }
        // Fallback: erste vorhandene Sektion
        return sections[0] || null;
    }

    function buildSection(genre, serverId) {
        var section = document.createElement('div');
        section.className = 'gr-genre-section verticalSection homeSection';
        section.style.cssText = 'margin-bottom: 1.5em;';

        // Titelzeile
        var titleDiv = document.createElement('div');
        titleDiv.className = 'sectionTitleContainer sectionTitleContainer-cards padded-left';
        var h2 = document.createElement('h2');
        h2.className = 'sectionTitle sectionTitle-cards';
        h2.textContent = genre.displayName;
        titleDiv.appendChild(h2);
        section.appendChild(titleDiv);

        // Karten-Container
        var cardContainer = document.createElement('div');
        cardContainer.className = 'itemsContainer scrollX hiddenScrollX padded-left padded-right';
        cardContainer.style.cssText = [
            'display: flex',
            'gap: 10px',
            'overflow-x: auto',
            'padding-bottom: 12px',
            'padding-top: 4px',
            'scrollbar-width: none'
        ].join(';');

        genre.items.forEach(function (item) {
            cardContainer.appendChild(buildCard(item, serverId));
        });

        section.appendChild(cardContainer);
        return section;
    }

    function buildCard(item, serverId) {
        var card = document.createElement('div');
        card.className = 'card portraitCard gr-card';
        card.style.cssText = 'min-width:130px;max-width:130px;flex-shrink:0;cursor:pointer;';

        var link = document.createElement('a');
        link.href = '#/details?id=' + item.id + '&serverId=' + serverId;
        link.style.cssText = 'text-decoration:none;color:inherit;display:block;';

        // Poster-Bild
        var imgWrapper = document.createElement('div');
        imgWrapper.style.cssText = [
            'position: relative',
            'padding-bottom: 150%',
            'background: #1c1c1c',
            'border-radius: 6px',
            'overflow: hidden',
            'transition: transform 0.2s ease'
        ].join(';');

        var img = document.createElement('img');
        img.src = ApiClient.getUrl('Items/' + item.id + '/Images/Primary', {
            maxHeight: 300,
            quality: 90
        });
        img.alt = item.name;
        img.loading = 'lazy';
        img.style.cssText = 'position:absolute;top:0;left:0;width:100%;height:100%;object-fit:cover;';
        img.onerror = function () {
            imgWrapper.style.background = '#2c2c2c';
            img.style.display = 'none';

            // Fallback-Text wenn kein Bild vorhanden
            var fallback = document.createElement('div');
            fallback.style.cssText = [
                'position:absolute;top:0;left:0;width:100%;height:100%',
                'display:flex;align-items:center;justify-content:center',
                'font-size:0.7em;text-align:center;padding:8px;color:#aaa'
            ].join(';');
            fallback.textContent = item.name;
            imgWrapper.appendChild(fallback);
        };

        // Hover-Effekt
        card.addEventListener('mouseenter', function () {
            imgWrapper.style.transform = 'scale(1.03)';
        });
        card.addEventListener('mouseleave', function () {
            imgWrapper.style.transform = 'scale(1)';
        });

        imgWrapper.appendChild(img);
        link.appendChild(imgWrapper);

        // Titeltext
        var footer = document.createElement('div');
        footer.style.cssText = 'padding:5px 0 0;';

        var title = document.createElement('div');
        title.className = 'cardText';
        title.style.cssText = 'font-size:0.78em;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;';
        title.title = item.name;
        title.textContent = item.name;
        footer.appendChild(title);

        if (item.year) {
            var year = document.createElement('div');
            year.className = 'cardText cardText-secondary';
            year.style.cssText = 'font-size:0.72em;color:#aaa;';
            year.textContent = item.year;
            footer.appendChild(year);
        }

        link.appendChild(footer);
        card.appendChild(link);
        return card;
    }

    waitForApiClient(init);
})();
