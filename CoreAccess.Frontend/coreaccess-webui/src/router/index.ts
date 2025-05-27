import {createRouter, createWebHistory} from 'vue-router';
import type {RouteRecordRaw} from 'vue-router';
import Dashboard from "../pages/Dashboard.vue";
import Login from "../pages/Login.vue";

const routes: RouteRecordRaw[] = [
    { path: '/', component: () => Dashboard },
    { path: '/login', component: () => Login},
];

export const router = createRouter({
    history: createWebHistory(),
    routes,
});