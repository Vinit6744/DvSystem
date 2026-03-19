# CORS Issue Fix

## Problem
The error "Redirect is not allowed for a preflight request" occurs because:
1. HTTPS redirection middleware was running before CORS middleware
2. Preflight OPTIONS requests were being redirected, which is not allowed

## Solution Applied

### 1. Reordered Middleware
- Moved `app.UseCors()` **before** `app.UseHttpsRedirection()`
- This ensures CORS headers are added to preflight requests before any redirect happens

### 2. Disabled HTTPS Redirection in Development
- Added condition to only use HTTPS redirection in production
- This prevents redirects during development when using HTTP

### 3. Improved CORS Configuration
- Added `SetPreflightMaxAge` to cache preflight requests
- Explicitly configured CORS policy

## How to Test

### Step 1: Restart Backend
```bash
cd DesignReview.API
dotnet run
```

**Important:** Make sure you're running the **HTTP profile** (port 5033), not HTTPS.

### Step 2: Verify Backend is Running
- Check terminal: Should show "Now listening on: http://localhost:5033"
- Open Swagger: http://localhost:5033/swagger

### Step 3: Restart Frontend
```bash
cd DesignReview.Frontend
npm start
```

### Step 4: Test Login
- Open http://localhost:4200
- Try logging in
- Check browser console - CORS error should be gone

## Configuration Summary

**Backend:**
- Port: `http://localhost:5033` (HTTP, not HTTPS)
- CORS: Allows `http://localhost:4200`
- HTTPS Redirection: Disabled in Development

**Frontend:**
- Port: `http://localhost:4200`
- API URL: `http://localhost:5033/api`

## If Issues Persist

1. **Clear browser cache** - CORS errors can be cached
2. **Check backend is running on HTTP** - Not HTTPS
3. **Verify CORS configuration** in `appsettings.json`:
   ```json
   "Cors": {
     "AllowedOrigins": ["http://localhost:4200"]
   }
   ```
4. **Check browser console** for exact error message
5. **Try incognito/private window** to rule out cache issues

## Testing CORS Manually

You can test CORS with curl:
```bash
curl -X OPTIONS http://localhost:5033/api/auth/login \
  -H "Origin: http://localhost:4200" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: content-type" \
  -v
```

Should return CORS headers without redirect.
