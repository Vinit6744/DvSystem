# Setup Instructions - Design Review System

## Backend Setup

### 1. Start the Backend API

Navigate to the backend directory and run:

```bash
cd DesignReview.API
dotnet run
```

Or use Visual Studio:
- Open `DesignReviewSystem.sln`
- Set `DesignReview.API` as startup project
- Press F5 or click Run

**Backend will run on:** `http://localhost:5033`

**Swagger UI:** `http://localhost:5033/swagger`

### 2. Verify Backend is Running

Open your browser and navigate to:
- `http://localhost:5033/swagger` - Should show Swagger UI
- `http://localhost:5033/api/auth/login` - Should return 400 (Bad Request) which is expected without body

## Frontend Setup

### 1. Install Dependencies

```bash
cd DesignReview.Frontend
npm install
```

### 2. Start Frontend Development Server

```bash
npm start
```

**Frontend will run on:** `http://localhost:4200`

### 3. Access the Application

Open your browser and navigate to:
- `http://localhost:4200`

## Troubleshooting Connection Issues

### Issue: ERR_CONNECTION_REFUSED

**Causes:**
1. Backend is not running
2. Wrong port in frontend configuration
3. CORS configuration issue

**Solutions:**

1. **Check if backend is running:**
   ```bash
   # Check if port 5033 is in use
   netstat -ano | findstr :5033
   ```

2. **Verify backend URL in frontend:**
   - Check `DesignReview.Frontend/src/environments/environment.ts`
   - Should be: `apiUrl: 'http://localhost:5033/api'`

3. **Check CORS configuration:**
   - Backend `appsettings.json` should have:
   ```json
   "Cors": {
     "AllowedOrigins": ["http://localhost:4200"]
   }
   ```

4. **Restart both services:**
   - Stop backend (Ctrl+C)
   - Stop frontend (Ctrl+C)
   - Start backend first: `dotnet run` in `DesignReview.API`
   - Then start frontend: `npm start` in `DesignReview.Frontend`

### Issue: CORS Error

If you see CORS errors in browser console:

1. Make sure `appsettings.json` has:
   ```json
   "Cors": {
     "AllowedOrigins": ["http://localhost:4200"]
   }
   ```

2. Restart the backend after changing CORS settings

3. Check browser console for exact CORS error message

## Default Login Credentials

You'll need to register a user first or check if there are seeded users in the database.

To register (requires Admin role):
- Use the register endpoint via Swagger or create an admin user directly in the database

## Port Configuration Summary

- **Backend API:** `http://localhost:5033`
- **Frontend:** `http://localhost:4200`
- **Swagger:** `http://localhost:5033/swagger`

## Quick Start Checklist

- [ ] Backend is running on port 5033
- [ ] Frontend environment.ts points to `http://localhost:5033/api`
- [ ] CORS allows `http://localhost:4200`
- [ ] Database is set up and migrations applied
- [ ] Frontend dependencies installed (`npm install`)
- [ ] Both services are running simultaneously
