# Design Review System - Frontend

Angular 18 frontend application for the Design Review System.

## Setup

1. Install dependencies:
```bash
npm install
```

2. Update API URL in `src/environments/environment.ts` if needed

3. Start development server:
```bash
npm start
```

The application will be available at `http://localhost:4200`

## Features

- JWT Authentication with interceptors
- Role-based routing guards
- Angular Material UI components
- Document upload and viewer
- Verification results display
- Review comments and summary
- Report export functionality

## Project Structure

- `src/app/core/` - Core services, interceptors, guards, models
- `src/app/shared/` - Shared components
- `src/app/features/` - Feature modules (auth, documents, verification, review)
- `src/app/layouts/` - Layout components
