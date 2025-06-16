import { coreAuth } from './coreAuth';
import type { CoreAccessConfig } from './types';

export function setupCoreAccess(config: CoreAccessConfig) {
    coreAuth.configure(config);
}

export { coreAuth };