import {createRouter, createWebHistory} from 'vue-router';
import type {RouteRecordRaw} from 'vue-router';
import Dashboard from "../pages/Dashboard.vue";
import Login from "../pages/Login.vue";
import Users from "../pages/Users.vue";
import Roles from "../pages/Roles.vue";
import Permissions from "../pages/Permissions.vue";
import SystemLogs from "../pages/SystemLogs.vue";
import AuditLogs from "../pages/AuditLogs.vue";
import AppSettings from "../pages/AppSettings.vue";
import {isAuthenticated} from "../services/AuthService.ts";
import MetricsHub from "../pages/MetricsHub.vue";
import InitialSetup from "../pages/InitialSetup/InitialSetup.vue";

const routes: RouteRecordRaw[] = [
    { path: '/', name: 'Dashboard', component: Dashboard, meta: { requiresAuth: true } },
    { path: '/login', name: 'Login', component: Login, meta: { public: true }, },
    { path: '/users', name: 'Users', component: Users, meta: { requiresAuth: true }, },
    { path: '/roles', name: 'Roles', component: Roles, meta: { requiresAuth: true }, },
    { path: '/permissions', name: 'Permissions', component: Permissions, meta: { requiresAuth: true }, },
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

router.beforeEach((to, from, next) => {
    const isPublic = to.meta.public === true

    if (!isAuthenticated.value && !isPublic) {
        next({
            path: "/login",
            query: { redirect: to.fullPath },
        })
    } else if (isAuthenticated.value && to.path === "/login") {
        const fallbackPath =
            from?.fullPath && from.fullPath !== "/login" ? from.fullPath : "/"
        next(fallbackPath)
    }else if(!isAuthenticated.value && to.path === "/login" && !to.query.redirect) {
        next({
            path: to.path,
            query: { redirect: '/' }
        })
    }else {
        next()
    }
})