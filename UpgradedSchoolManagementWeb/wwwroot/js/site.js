// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(function () {
    const MOBILE_BP = 991.98;
    const $sidebar = $('#sidebar');
    const $main = $('#main');
    const $overlay = $('#overlay');
    const $html = $('html');

    function isMobile() { return $(window).width() <= MOBILE_BP; }

    /* ════ SIDEBAR TOGGLE ════ */
    $('#sidebarToggle').on('click', function () {
        if (isMobile()) {
            $sidebar.toggleClass('mobile-open');
            $overlay.toggleClass('show');
        } else {
            const collapsing = !$sidebar.hasClass('collapsed');
            $sidebar.toggleClass('collapsed');
            $main.toggleClass('sidebar-collapsed');
            // Close all open submenus when collapsing
            if (collapsing) {
                $('.nav-submenu.open').removeClass('open');
                $('.nav-item-link.open').removeClass('open');
            }
        }
    });

    $overlay.on('click', function () {
        $sidebar.removeClass('mobile-open');
        $overlay.removeClass('show');
    });

    $(window).on('resize', function () {
        if (!isMobile()) {
            $sidebar.removeClass('mobile-open');
            $overlay.removeClass('show');
        }
    });

    /* ════ SUBMENU ACCORDION ════ */
    $(document).on('click', '.nav-item-link.has-children', function (e) {
        e.preventDefault();

        // Expand sidebar first if collapsed (desktop)
        if (!isMobile() && $sidebar.hasClass('collapsed')) {
            $sidebar.removeClass('collapsed');
            $main.removeClass('sidebar-collapsed');
            const $self = $(this);
            setTimeout(function () { toggleSub($self); }, 320);
            return;
        }
        toggleSub($(this));
    });

    function toggleSub($link) {
        const $sub = $link.next('.nav-submenu');
        const isOpen = $sub.hasClass('open');

        // Accordion: close others
        $('.nav-submenu.open').not($sub).removeClass('open');
        $('.nav-item-link.has-children.open').not($link).removeClass('open');

        $sub.toggleClass('open', !isOpen);
        $link.toggleClass('open', !isOpen);
    }

    /* ════ ACTIVE STATE ════ */
    function setActive($el, isChild) {
        // Remove all active states
        $('.nav-item-link').removeClass('active parent-active');
        $('.nav-child-link').removeClass('active');

        if (isChild) {
            $el.addClass('active');
            // Mark parent link as parent-active and keep submenu open
            const $parentLink = $el.closest('.nav-submenu').siblings('.nav-item-link.has-children');
            $parentLink.addClass('parent-active open');
            $el.closest('.nav-submenu').addClass('open');
        } else {
            $el.addClass('active');
        }
    }

    function markActiveByUrl() {
        const path = window.location.pathname.toLowerCase();

        // Check child links first
        let matched = false;
        $('.nav-child-link').each(function () {
            const href = ($(this).attr('href') || '').toLowerCase();
            if (href && path.startsWith(href)) {
                setActive($(this), true);
                matched = true;
                return false;
            }
        });

        if (!matched) {
            // Check parent links (no children)
            $('.nav-item-link:not(.has-children)').each(function () {
                const href = ($(this).attr('href') || '').toLowerCase();
                if (href && path.startsWith(href)) {
                    setActive($(this), false);
                    return false;
                }
            });
        }
    }

    // Child links — navigate normally, just mark active
    $(document).on('click', '.nav-child-link', function () {
        setActive($(this), true);
        // Mobile: close sidebar
        if (isMobile()) {
            $sidebar.removeClass('mobile-open');
            $overlay.removeClass('show');
        }
    });

    // Leaf parent links — navigate normally, just mark active
    $(document).on('click', '.nav-item-link:not(.has-children)', function () {
        setActive($(this), false);
        // Mobile: close sidebar
        if (isMobile()) {
            $sidebar.removeClass('mobile-open');
            $overlay.removeClass('show');
        }
    });

    /* ════ THEME TOGGLE ════ */
    function applyTheme(theme) {
        $html.attr('data-theme', theme);
        const $icon = $('#themeKnobIcon');
        if (theme === 'light') {
            $icon.removeClass('bi-moon-stars-fill').addClass('bi-sun-fill');
        } else {
            $icon.removeClass('bi-sun-fill').addClass('bi-moon-stars-fill');
        }
        try { localStorage.setItem('edusphere-theme', theme); } catch (e) { }
    }

    // Restore saved theme
    let saved = 'dark';
    try { saved = localStorage.getItem('edusphere-theme') || 'dark'; } catch (e) { }
    applyTheme(saved);

    $('#themeToggle').on('click', function () {
        applyTheme($html.attr('data-theme') === 'dark' ? 'light' : 'dark');
    });

    /* ════ PROFILE DROPDOWN ════ */
    $('#profileTrigger').on('click', function (e) {
        e.stopPropagation();
        $('#profileWrap').toggleClass('open');
    });

    // Close on outside click
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#profileWrap').length) {
            $('#profileWrap').removeClass('open');
        }
    });

    // Close on ESC
    $(document).on('keydown', function (e) {
        if (e.key === 'Escape') $('#profileWrap').removeClass('open');
    });

    // Prevent dropdown clicks from bubbling to document
    $('#profileDropdown').on('click', function (e) { e.stopPropagation(); });
});