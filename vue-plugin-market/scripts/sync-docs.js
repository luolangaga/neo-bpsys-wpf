import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const projectRoot = path.resolve(__dirname, '..');
const sourceDir = path.resolve(projectRoot, '../docs');
const targetDir = path.resolve(projectRoot, 'public/docs');

console.log(`Source: ${sourceDir}`);
console.log(`Target: ${targetDir}`);

if (!fs.existsSync(sourceDir)) {
    console.error('Source docs directory not found!');
    process.exit(1);
}

// Function to recursively copy and build tree
function processDirectory(currentPath, relativePath = '') {
    const items = fs.readdirSync(currentPath, { withFileTypes: true });
    const structure = [];

    // Ensure target directory exists
    const currentTargetDir = path.join(targetDir, relativePath);
    if (!fs.existsSync(currentTargetDir)) {
        fs.mkdirSync(currentTargetDir, { recursive: true });
    }

    for (const item of items) {
        const itemRelativePath = path.join(relativePath, item.name);
        const sourceItemPath = path.join(currentPath, item.name);
        const targetItemPath = path.join(targetDir, itemRelativePath);

        if (item.isDirectory()) {
            const children = processDirectory(sourceItemPath, itemRelativePath);
            structure.push({
                name: item.name,
                type: 'directory',
                path: itemRelativePath.replace(/\\/g, '/'), // Normalize paths for web
                children: children
            });
        } else if (item.isFile() && item.name.endsWith('.md')) {
            fs.copyFileSync(sourceItemPath, targetItemPath);
            structure.push({
                name: item.name,
                type: 'file',
                path: itemRelativePath.replace(/\\/g, '/') // Normalize paths for web
            });
        }
    }
    return structure;
}

// Clean up public/docs if it exists to remove old files? 
// For simplicity, we just overwrite.

try {
    const manifest = processDirectory(sourceDir);

    fs.writeFileSync(
        path.join(targetDir, 'manifest.json'),
        JSON.stringify(manifest, null, 2)
    );

    console.log('Docs synced and manifest generated successfully.');
} catch (error) {
    console.error('Error syncing docs:', error);
    process.exit(1);
}
