export function generateRandomBase64Key(length: number = 32): string {
    const bytes = new Uint8Array(length)
    crypto.getRandomValues(bytes)
    return btoa(String.fromCharCode(...bytes))
}
