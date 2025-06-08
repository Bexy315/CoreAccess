```markdown
# @coreaccess/client

CoreAccess Client package for interacting with the CoreAccess API.

## Installation

Install the package via npm:

```bash
npm install @coreaccess/client
```

## Usage

### Configuration

Configure the client with the base URL of your CoreAccess API:

```typescript
import { coreAuth } from '@coreaccess/client';

coreAuth.configure({
    baseUrl: 'https://api.example.com',
});
```

### Authentication

#### Login

Authenticate a user with their credentials:

```typescript
await coreAuth.login({ username: 'user', password: 'password' });
```

#### Logout

Log out the current user:

```typescript
coreAuth.logout();
```

#### Check Authentication Status

Check if a user is logged in:

```typescript
const isLoggedIn = coreAuth.isLoggedIn();
```

#### Get Current User

Retrieve the currently authenticated user:

```typescript
const user = coreAuth.getCurrentUser();
```

### Event Listeners

Listen for authentication changes:

```typescript
coreAuth.onAuthChange((isAuthenticated) => {
    console.log('Authentication status changed:', isAuthenticated);
});

coreAuth.onLogin(() => {
    console.log('User logged in');
});

coreAuth.onLogout(() => {
    console.log('User logged out');
});
```

## Development

Build the package:

```bash
npm run build
```

## Dependencies

- `axios`: HTTP client for API requests.

## License

MIT
```