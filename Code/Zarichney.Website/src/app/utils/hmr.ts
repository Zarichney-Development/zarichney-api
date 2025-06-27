import { ApplicationRef } from '@angular/core';
import { createNewHosts } from '@angularclass/hmr';

export const hmrBootstrap = (
    module: any,
    bootstrapFn: () => Promise<ApplicationRef>
) => {
    let appRef: ApplicationRef;

    module.hot.accept();

    bootstrapFn().then(ref => {
        appRef = ref;
    });

    module.hot.dispose(() => {
        const elements = appRef.components.map(c => c.location.nativeElement);
        const removeOldHosts = createNewHosts(elements);
        appRef.destroy();
        removeOldHosts();
    });
};
