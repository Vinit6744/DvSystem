# Node.js Version Requirements

## Required Node.js Version

For the Angular 18 frontend project, you need:

**Node.js: ^18.19.1 || ^20.11.1 || ^22.0.0**

### Recommended Versions:
- **Node.js 20.11.1** (LTS - Recommended)
- **Node.js 22.x** (Latest)
- **Node.js 18.19.1+** (Minimum supported)

### npm Version:
- **npm >= 9.0.0**

## Check Your Current Version

```bash
node --version
npm --version
```

## Installation Options

### Option 1: Using NVM (Node Version Manager) - Recommended

**Windows:**
```bash
# Install nvm-windows from: https://github.com/coreybutler/nvm-windows
nvm install 20.11.1
nvm use 20.11.1
```

**Mac/Linux:**
```bash
# Install nvm from: https://github.com/nvm-sh/nvm
nvm install 20.11.1
nvm use 20.11.1
```

### Option 2: Direct Download

Download and install from: https://nodejs.org/
- Choose **LTS version (20.x)** for stability
- Or **Current version (22.x)** for latest features

## Verify Installation

After installing, verify:
```bash
node --version  # Should show v20.11.1 or higher
npm --version   # Should show 9.x.x or higher
```

## Using .nvmrc File

If you're using NVM, the project includes a `.nvmrc` file. Simply run:
```bash
nvm use
```

This will automatically switch to the correct Node.js version specified in `.nvmrc`.

## Troubleshooting

If you encounter version issues:
1. Make sure you're using a supported Node.js version
2. Clear npm cache: `npm cache clean --force`
3. Delete `node_modules` and `package-lock.json`
4. Run `npm install` again
