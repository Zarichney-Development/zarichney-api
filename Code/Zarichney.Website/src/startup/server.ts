import { fileURLToPath } from 'url';
import { CommonEngine } from '@angular/ssr/node';
import express from 'express';
import { join as joinPath, resolve, dirname } from 'path';
import { APP_BASE_HREF } from '@angular/common';
import fs from 'fs';
import { REQUEST } from '../app/utils/request.token';

const currentDir = dirname(fileURLToPath(import.meta.url));
// For prerender: [PROJECT_ROOT]/.angular/prerender-root/[guid]/src -> [PROJECT_ROOT]
const projectRoot = currentDir.includes('.angular')
  ? resolve(currentDir, '../../../')
  : resolve(currentDir, '../../');
const routesPath = joinPath(projectRoot, 'src', 'app', 'routes', 'ssr.routes.txt');
const distFolder = joinPath(projectRoot, 'dist');
const serverDistFolder = joinPath(distFolder, 'server');
const browserDir = joinPath(distFolder, 'browser');
const indexHtml = joinPath(serverDistFolder, 'index.server.html');

// Add this function after the imports
function normalizePath(path: string): string {
  // Remove query parameters
  const pathWithoutQuery = path.split('?')[0];
  // Remove hash
  const pathWithoutHash = pathWithoutQuery.split('#')[0];
  // Remove leading and trailing slashes
  return pathWithoutHash.replace(/^\/+|\/+$/g, '');
}

// Load SSR routes
const ssrRoutes = new Set<string>();
try {
  const routesContent = fs.readFileSync(routesPath, "utf-8");
  routesContent.split("\n").forEach((route) => {
    const trimmedRoute = route.trim();
    if (trimmedRoute) {
      ssrRoutes.add(normalizePath(trimmedRoute));
    }
  });
  console.log("SSR routes loaded:", Array.from(ssrRoutes).map(r => r === '' ? '/' : `/${r}`));
} catch (error) {
  console.warn("Could not load SSR routes:", error);
}

// The Express app is exported so that it can be used by serverless Functions.
export function app(): express.Express {
  console.log("Creating server...");

  const server = express();

  server.set('view engine', 'html');
  server.set('views', browserDir);

  // Serve static files
  server.use(express.static(browserDir, {
    maxAge: '1y',
    index: false
  }));

  // All regular routes use the Universal engine
  server.get('*', async (req, res, next) => {
    console.log("Handling request for:", req.url);

    const normalizedPath = normalizePath(req.url);

    try {
      if (ssrRoutes.has(normalizedPath)) {
        const prerenderPath = joinPath(distFolder, 'browser', normalizedPath, "index.html");

        if (fs.existsSync(prerenderPath)) {
          console.log(`Serving prerendered file: ${prerenderPath}`);
          return res.sendFile(prerenderPath);
        }
        console.log(`Prerendered file not found at: ${prerenderPath}`);
      }

      // Load the CommonEngine to bootstrap and render the application
      const commonEngine = new CommonEngine();
      const bootstrap = (await import('./app.server')).default;

      const html = await commonEngine.render({
        bootstrap,
        documentFilePath: indexHtml,
        publicPath: browserDir,
        url: req.url,
        providers: [
          { provide: APP_BASE_HREF, useValue: req.baseUrl },
          { provide: REQUEST, useValue: req }
        ]
      });

      res.send(html);
    } catch (error) {
      console.error("Error in request handler:", error);
      next(error);
    }
  });

  return server;
}

export default app;

function run(): void {
  console.log("Starting server...");
  const port = process.env['PORT'] || 4200;
  try {
    // Start up the Node server
    const server = app();

    server.use((req, res, next) => {
      console.log(`Request received: ${req.method} ${req.url}`);
      next();
    });

    server.listen(port, () => {
      console.log(`Node Express server listening on http://localhost:${port}`);
    });
  } catch (error) {
    console.error("Error starting server:", error);
  }
}

// Only call run() if this file is the main module.
import { pathToFileURL } from 'url';
if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  run();
}
