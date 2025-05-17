(function() {
    console.log("Swagger UI enhancement initialized");

    // Track which elements should have the unavailable class
    // We'll use a Set instead of a WeakMap for simpler iteration
    const warningElements = new Set();

    // Step 1 & 2: Wait for elements to be rendered and apply initial classes
    function initializeWarningClasses() {
        // Find all operation blocks that contain the warning emoji in their summary
        const operations = document.querySelectorAll('.opblock-summary-description');

        if (operations.length === 0) {
            // Elements not found yet, try again soon
            setTimeout(initializeWarningClasses, 100);
            return;
        }

        console.log("Swagger UI elements found, applying initial classes");

        // Apply unavailable class to operations with warning emoji
        operations.forEach(function(op) {
            if (op.textContent.includes('⚠️')) {
                const opblock = op.closest('.opblock');
                if (opblock) {
                    opblock.classList.add('unavailable');
                    // Track this element for future reference
                    warningElements.add(opblock);
                }
            }
        });

        // Step 3: Set up watchers on these elements
        setupWatchers();
    }

    // Step 3: Set up watchers on the parent container
    function setupWatchers() {
        // Use a mutation observer on the Swagger UI container to detect changes
        const swaggerContainer = document.getElementById('swagger-ui') ||
            document.querySelector('.swagger-ui') ||
            document.body;

        const observer = new MutationObserver(function() {
            // Step 4: Check if any of our marked elements lost their class
            reapplyRemovedClasses();

            // Also check for newly added elements with warning emoji
            checkForNewWarningElements();
        });

        // Watch for both child changes and attribute changes
        observer.observe(swaggerContainer, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: ['class']
        });

        console.log("Watchers initialized on Swagger UI container");
    }

    // Step 4: Reapply unavailable class when it has been removed
    function reapplyRemovedClasses() {
        // Iterate through our tracked elements using Set's forEach
        warningElements.forEach(function(opblock) {
            // Check if the element still exists in the DOM
            if (document.contains(opblock)) {
                // If it exists but doesn't have the class, reapply it
                if (!opblock.classList.contains('unavailable')) {
                    opblock.classList.add('unavailable');
                    console.log("Reapplied 'unavailable' class to an element that lost it");
                }
            } else {
                // Element no longer in DOM, remove from our set
                warningElements.delete(opblock);
            }
        });
    }

    // Check for newly added elements that should be marked
    function checkForNewWarningElements() {
        const operations = document.querySelectorAll('.opblock-summary-description');

        operations.forEach(function(op) {
            if (op.textContent.includes('⚠️')) {
                const opblock = op.closest('.opblock');
                // Use has() method for checking Set membership
                if (opblock && !warningElements.has(opblock)) {
                    opblock.classList.add('unavailable');
                    warningElements.add(opblock);
                    console.log("Found and marked new warning element");
                }
            }
        });
    }

    // Additional safeguard - listen for specific user interactions
    function setupInteractionListeners() {
        // Listen for clicks that might cause UI changes
        document.addEventListener('click', function(e) {
            // If clicked on expand/collapse buttons or try-out buttons
            if (e.target.closest('.opblock-tag') ||
                e.target.closest('.opblock-summary') ||
                e.target.closest('.try-out__btn') ||
                e.target.closest('.auth__btn')) {

                // Wait for any UI updates to complete
                setTimeout(function() {
                    reapplyRemovedClasses();
                    checkForNewWarningElements();
                }, 300);
            }
        });
    }

    // Start the initialization process
    function start() {
        // First wait for DOM to be ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initializeWarningClasses);
        } else {
            initializeWarningClasses();
        }

        // Set up interaction listeners
        setupInteractionListeners();

        // Also try again on full load as a fallback
        window.addEventListener('load', function() {
            // If we haven't found elements yet, try again
            if (warningElements.size === 0) {
                initializeWarningClasses();
            }
        });
    }

    // Begin the process
    start();
})();
