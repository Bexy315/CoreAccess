import { useToast } from 'primevue/usetoast';

let globalToast: ReturnType<typeof useToast>;

export function registerGlobalToast(instance: ReturnType<typeof useToast>) {
    globalToast = instance;
}

export function showError(message: string, summary = 'Error') {
    globalToast?.add({
        severity: 'error',
        summary,
        detail: message,
        life: 5000
    });
}

export function showSuccess(message: string, summary = 'Success') {
    globalToast?.add({
        severity: 'success',
        summary,
        detail: message,
        life: 3000
    });
}
