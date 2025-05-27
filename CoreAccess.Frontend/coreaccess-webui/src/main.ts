import { createApp } from 'vue';
import App from './App.vue';
import { createPinia } from 'pinia';
import { router } from './router';

// PrimeVue & Theme
import PrimeVue from 'primevue/config';
import 'primevue/resources/themes/aura-dark-blue/theme.css'; // oder ein anderes dark theme
import 'primevue/resources/primevue.min.css';
import 'primeicons/primeicons.css';

// Tailwind
import './assets/tailwind.css';

// Optional globale Komponenten
import Menubar from 'primevue/menubar';
import PanelMenu from 'primevue/panelmenu';
import Menu from 'primevue/menu';
import Avatar from 'primevue/avatar';
import Button from 'primevue/button';

const app = createApp(App);

app.use(createPinia());
app.use(router);
app.use(PrimeVue, { ripple: true });

app.component('Menubar', Menubar);
app.component('PanelMenu', PanelMenu);
app.component('Menu', Menu);
app.component('Avatar', Avatar);
app.component('Button', Button);

app.mount('#app');
