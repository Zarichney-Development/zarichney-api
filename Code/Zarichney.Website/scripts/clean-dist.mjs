import { rm } from 'fs/promises';
import { resolve, relative } from 'path';
import { fileURLToPath } from 'url';
import { cwd } from 'process';

const __dirname = fileURLToPath(new URL('.', import.meta.url));
const projectRoot = resolve(__dirname, '..');
const distPath = resolve(projectRoot, 'dist');

async function clean() {
    const currentDir = cwd();
    const relativeToProject = relative(distPath, currentDir);
    
    if (!relativeToProject.startsWith('..')) {
        console.error('\nError: Cannot clean while inside dist directory.');
        console.error('Please run these commands from the project root:');
        console.error(`cd ${projectRoot}`);
        console.error('npm run clean');
        console.error('npm run local:build\n');
        process.exit(1);
    }

    try {
        console.log('Cleaning dist folder...');
        
        // Try multiple times with delay in case of EBUSY
        for (let i = 0; i < 3; i++) {
            try {
                await rm(distPath, { recursive: true, force: true });
                console.log('Dist folder cleaned successfully');
                return;
            } catch (error) {
                if (error.code === 'EBUSY' && i < 2) {
                    console.log('Folder busy, retrying in 1 second...');
                    await new Promise(resolve => setTimeout(resolve, 1000));
                    continue;
                }
                throw error;
            }
        }
    } catch (error) {
        if (error.code === 'ENOENT') {
            console.log('Dist folder does not exist, skipping clean');
        } else {
            console.error('\nError cleaning dist folder:', error);
            console.error('\nPlease try:');
            console.error('1. Close any programs that might be using the dist folder');
            console.error('2. Run these commands from the project root:');
            console.error(`cd ${projectRoot}`);
            console.error('npm run clean');
            console.error('npm run local:build\n');
            process.exit(1);
        }
    }
}

clean();
