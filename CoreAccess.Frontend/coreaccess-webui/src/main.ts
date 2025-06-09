import { createApp } from 'vue';
import App from './App.vue';
import { createPinia } from 'pinia';
import { router } from './router';

import PrimeVue from 'primevue/config';
import ToastService from 'primevue/toastservice';
import 'primeicons/primeicons.css';

import VueSidebarMenu from "vue-sidebar-menu";
import "vue-sidebar-menu/dist/vue-sidebar-menu.css";

import './assets/tailwind.css';

import Menubar from 'primevue/menubar';
import PanelMenu from 'primevue/panelmenu';
import Menu from 'primevue/menu';
import Avatar from 'primevue/avatar';
import Button from 'primevue/button';
import Password from "primevue/password";
import ProgressSpinner from 'primevue/progressspinner';
import Toast from "primevue/toast";
import Card from 'primevue/card'
import InputText from 'primevue/inputtext'
import Breadcrumb from "primevue/breadcrumb";
import Tooltip from "primevue/tooltip";
import Skeleton from "primevue/skeleton";

import CoreAccessPreset from "./assets/theme.ts";

import Sidebar from './layouts/Sidebar.vue'
import Topbar from './layouts/Topbar.vue';

const app = createApp(App);

app.use(createPinia());
app.use(router);
app.use(PrimeVue, { theme: {
    preset: CoreAccessPreset,
    options: {
        ripple: true,
        darkModeSelector: 'none',
    }
}});
app.use(ToastService);

app.use(VueSidebarMenu);

app.component('Menubar', Menubar);
app.component('PanelMenu', PanelMenu);
app.component('Menu', Menu);
app.component('Avatar', Avatar);
app.component('Button', Button);
app.component('Password', Password);
app.component('ProgressSpinner', ProgressSpinner);
app.component('Toast', Toast);
app.component('Card', Card);
app.component('InputText', InputText);
app.component('Breadcrumb', Breadcrumb);
app.component('Skeleton', Skeleton);
app.directive('Tooltip', Tooltip);

app.component('Sidebar', Sidebar);
app.component('Topbar', Topbar);

app.mount('#app');
