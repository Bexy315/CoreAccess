import { defineStore } from 'pinia';

export const useAppStateStore = defineStore('appState', {
  state: () => ({
    isLoading: false,
    isInitiated: false,
  }),
  actions: {
    setLoading(loading: boolean) {
      this.isLoading = loading;
    },
    setInitiated(initiated: boolean) {
      this.isInitiated = initiated;
    },
  },
});