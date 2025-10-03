import {createRouter, createWebHistory} from 'vue-router';
import type {RouteRecordRaw} from 'vue-router';
import Dashboard from "../pages/Dashboard.vue";
import Users from "../pages/Users.vue";
import Roles from "../pages/Roles.vue";
import Permissions from "../pages/Permissions.vue";
import SystemLogs from "../pages/SystemLogs.vue";
import AuditLogs from "../pages/AuditLogs.vue";
import AppSettings from "../pages/AppSettings.vue";
import {isAuthenticated, login} from "../services/auth.ts";
import MetricsHub from "../pages/MetricsHub.vue";
import InitialSetup from "../pages/InitialSetup/InitialSetup.vue";
import Callback from "../pages/Callback.vue";
import UserDetailDialogWrapper from "../components/dialogs/Users/UserDetailDialogWrapper.vue";
import Clients from "../pages/Clients.vue";
import ApplicationDetailDialogWrapper from "../components/dialogs/Applications/ApplicationDetailDialogWrapper.vue";
import RoleDetailDialogWrapper from "../components/dialogs/Roles/RoleDetailDialogWrapper.vue";

const routes: RouteRecordRaw[] = [
    { path: '/', name: 'Dashboard', component: Dashboard, meta: { requiresAuth: true, public: false } },
    { path: '/callback', name: 'Callbafck', component: Callback, meta: { public: true }, },
    { path: '/users', name: 'Users', component: Users, meta: { requiresAuth: true },
        children: [{ path: ':id', component: UserDetailDialogWrapper } ]},
    { path: '/roles', name: 'Roles', component: Roles, meta: { requiresAuth: true },
        children: [{ path: ':id', component: RoleDetailDialogWrapper } ]},
    { path: '/permissions', name: 'Permissions', component: Permissions, meta: { requiresAuth: true }, },
    { path: '/clients', name: 'Clients', component: Clients, meta: { requiresAuth: true },
        children: [{ path: ':id', component: ApplicationDetailDialogWrapper } ]},
    { path: '/settings', name: 'AppSettings', component: AppSettings, meta: { requiresAuth: true }, },
    { path: '/metrics', name: 'MetricsHub', component: MetricsHub, meta: { requiresAuth: true }, },
    { path: '/metrics/logs/system', name: 'SystemLogs', component: SystemLogs, meta: { requiresAuth: true }, },
    { path: '/metrics/logs/audit', name: 'AuditLogs', component: AuditLogs, meta: { requiresAuth: true }, },
    { path: '/initial-setup', name: 'InitialSetup', component: InitialSetup, meta: { public: true } },
    { path: '/dashboard', redirect: '/' },
    { path: '/:pathMatch(.*)*', redirect: '/' }
];

export const router = createRouter({
    history: createWebHistory(),
    routes,
});

router.beforeEach((to, _, next) => {
    const isPublic = to.meta.public === true
    if (!isAuthenticated() && !isPublic) {
            login(to.fullPath)
    }else {
        next()
    }
})