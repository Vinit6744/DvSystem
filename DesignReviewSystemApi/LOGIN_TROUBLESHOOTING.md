# Login Troubleshooting Guide

## Issue: Login button doesn't navigate after clicking

### Possible Causes:
1. **Backend response structure mismatch** - Backend returns different structure than frontend expects
2. **Authentication state not updating** - Auth guard blocking navigation
3. **API call failing silently** - Error not being displayed
4. **Navigation error** - Router navigation failing

## Fixes Applied:

### 1. Updated Login Response Model
- Backend returns: `Token`, `UserName`, `UserId`, `Roles`
- Frontend now handles both PascalCase and camelCase

### 2. Improved Error Handling
- Added console logging to debug login flow
- Better error messages displayed to user
- Handles missing token in response

### 3. Enhanced User Object Creation
- Converts backend response to frontend User model
- Handles role extraction from Roles array

## Debugging Steps:

### Step 1: Check Browser Console
1. Open browser DevTools (F12)
2. Go to Console tab
3. Try logging in
4. Look for:
   - "Login response:" - Should show the API response
   - "Setting user:" - Should show user object being set
   - "Authentication state:" - Should show `true`
   - Any error messages

### Step 2: Check Network Tab
1. Open DevTools → Network tab
2. Try logging in
3. Find the `/api/auth/login` request
4. Check:
   - Status code (should be 200)
   - Response body (should have Token, UserName, UserId, Roles)
   - Request payload (should have userName and password)

### Step 3: Verify Backend Response
The backend should return:
```json
{
  "token": "eyJhbGc...",
  "expiresAt": "2024-01-01T12:00:00Z",
  "userName": "admin",
  "userId": "guid-here",
  "roles": ["Admin"]
}
```

### Step 4: Check Local Storage
After login, check browser localStorage:
1. Open DevTools → Application tab → Local Storage
2. Should see:
   - `jwt_token` - The JWT token
   - `user_data` - The user object as JSON

## Common Issues:

### Issue: "No token received from server"
**Solution:** Check backend is returning Token in response. Verify API endpoint is working in Swagger.

### Issue: Navigation happens but redirects back to login
**Solution:** Check auth guard - user might not have Reviewer/Admin role. Check `isReviewer()` method.

### Issue: Button stays disabled
**Solution:** Form validation issue - check browser console for form errors.

### Issue: API call fails with CORS error
**Solution:** Make sure backend CORS is configured correctly and backend is running.

## Testing:

1. **Test with Swagger:**
   - Go to `http://localhost:5033/swagger`
   - Try POST `/api/auth/login`
   - Verify response structure

2. **Test Frontend:**
   - Open `http://localhost:4200`
   - Open browser console
   - Try logging in
   - Check console logs and network requests

3. **Verify Authentication:**
   - After login, check localStorage
   - Try accessing `/documents` directly
   - Should not redirect to login

## Next Steps if Still Not Working:

1. Check backend logs for errors
2. Verify database has user with correct credentials
3. Check if user has proper roles assigned
4. Verify JWT token is being generated correctly
5. Check auth guard logic
