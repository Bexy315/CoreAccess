import apiClient from "./apiClient.ts";

export async function sendInitialSetupData(
  initialSetupData: Record<string, any>
): Promise<void> {
  try {
    await apiClient.post('/setup', initialSetupData);
  } catch (error) {
    console.error('Failed to send initial setup data:', error);
    throw error;
  }
}