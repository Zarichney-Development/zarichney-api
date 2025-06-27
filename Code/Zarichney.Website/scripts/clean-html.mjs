import { readFile, writeFile, readdir } from 'fs/promises';
import { resolve, dirname, join } from 'path';
import { fileURLToPath } from 'url';
import { JSDOM } from 'jsdom';

const __dirname = dirname(fileURLToPath(import.meta.url));
const projectRoot = resolve(__dirname, '..');

// Component transformation map
const componentTransforms = {
    'app-root': {
        tag: 'div',
        attrs: { class: 'app-container' }
    },
    'test-screen': {
        tag: 'main',
        attrs: { class: 'test-page' }
    },
    'app-menu': {
        tag: 'nav',
        attrs: { class: 'main-navigation' }
    },
    'router-outlet': {
        remove: true
    }
};

// Semantic attribute mapping
const attributeTransforms = {
    'routerlink': 'href',
    'routerlinkactive': 'data-active',
    'ariacurrentwhenactive': 'aria-current'
};

async function transformElement(element) {
    const tagName = element.tagName.toLowerCase();
    const transform = componentTransforms[tagName];

    if (transform) {
        if (transform.remove) {
            element.remove();
            return;
        }

        // Create new element with proper semantic tag
        const newElement = element.ownerDocument.createElement(transform.tag);
        
        // Copy content
        newElement.innerHTML = element.innerHTML;
        
        // Apply semantic attributes
        if (transform.attrs) {
            Object.entries(transform.attrs).forEach(([key, value]) => {
                newElement.setAttribute(key, value);
            });
        }

        // Replace original element
        element.replaceWith(newElement);
        return newElement;
    }

    return element;
}

async function cleanHtml(filePath) {
    console.log(`Processing: ${filePath}`);
    const html = await readFile(filePath, 'utf-8');
    const dom = new JSDOM(html);
    const { document } = dom.window;

    // Transform Angular components to semantic HTML
    const components = document.querySelectorAll('*');
    for (const element of components) {
        await transformElement(element);
    }

    // Clean up attributes
    document.querySelectorAll('*').forEach(element => {
        const attrs = Array.from(element.attributes);
        attrs.forEach(attr => {
            // Transform known attributes
            if (attributeTransforms[attr.name.toLowerCase()]) {
                const newAttrName = attributeTransforms[attr.name.toLowerCase()];
                element.setAttribute(newAttrName, attr.value);
                element.removeAttribute(attr.name);
            }
            // Remove Angular-specific attributes
            else if (attr.name.startsWith('_ng') || 
                    attr.name.startsWith('ng-') ||
                    attr.name.startsWith('ng-reflect-')) {
                element.removeAttribute(attr.name);
            }
        });
    });

    // Remove scripts and unused styles
    document.querySelectorAll('script').forEach(script => script.remove());
    document.querySelectorAll('style[ng-app-id]').forEach(style => style.remove());

    // Clean up comments and empty text nodes
    const clean = dom.serialize()
        .replace(/<!--.*?-->/g, '')
        .replace(/^\s*[\r\n]/gm, '');

    await writeFile(filePath, clean);
    console.log(`Cleaned: ${filePath}`);
}

async function findHtmlFiles(dir) {
    const entries = await readdir(dir, { withFileTypes: true });
    const files = await Promise.all(entries.map(entry => {
        const path = resolve(dir, entry.name);
        return entry.isDirectory() ? findHtmlFiles(path) : path;
    }));
    return files.flat().filter(file => file.endsWith('.html'));
}

async function processPrerenderedFiles() {
    try {
        const distFolder = resolve(projectRoot, 'dist/browser');
        const htmlFiles = await findHtmlFiles(distFolder);
        
        console.log('Found HTML files:', htmlFiles);
        
        for (const file of htmlFiles) {
            await cleanHtml(file);
        }
        
        console.log('All files processed successfully');
    } catch (error) {
        console.error('Error processing files:', error);
        process.exit(1);
    }
}

processPrerenderedFiles();
