import apiClient from "./apiClient.ts";


export async function PingBackend() {
    try {
        const response = await apiClient.get('/debug/ping', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
        });

        if (response.status !== 200) {
            throw new Error(`Error: ${response.statusText}`);
        }
        return response.data;
    } catch (error) {
        console.error('PingBackend error:', error);
        throw error;
    }
}