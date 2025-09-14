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

import PanelMenu from 'primevue/panelmenu';
import Menu from 'primevue/menu';
import Menubar from 'primevue/menubar';
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
import Stepper from 'primevue/stepper';
import StepList from 'primevue/steplist';
import StepPanels from 'primevue/steppanels';
import StepItem from 'primevue/stepitem';
import Step from 'primevue/step';
import StepPanel from 'primevue/steppanel';
import Checkbox from 'primevue/checkbox';
import Tag from 'primevue/tag';
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import Divider from "primevue/divider";
import Accordion from 'primevue/accordion';
import AccordionPanel from 'primevue/accordionpanel';
import AccordionHeader from 'primevue/accordionheader';
import AccordionContent from 'primevue/accordioncontent';
import Select from 'primevue/select';
import Dialog from "primevue/dialog";
import InputNumber from 'primevue/inputnumber';
import InputSwitch from 'primevue/toggleswitch';
import MultiSelect from 'primevue/multiselect';
import ConfirmDialog from 'primevue/confirmdialog';
import ConfirmationService from 'primevue/confirmationservice'
import Tabs from 'primevue/tabs';
import TabList from 'primevue/tablist';
import Tab from 'primevue/tab';
import TabPanels from 'primevue/tabpanels';
import TabPanel from 'primevue/tabpanel';
import PickList from 'primevue/picklist';


import AddUserDialog from "./components/dialogs/AddUserDialog.vue";
import UserDetailDialogWrapper from "./components/dialogs/UserDetailDialogWrapper.vue";

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
app.use(ConfirmationService);

app.use(VueSidebarMenu);

app.component('PanelMenu', PanelMenu);
app.component('Menu', Menu);
app.component('Menubar', Menubar);
app.component('Avatar', Avatar);
app.component('Button', Button);
app.component('Password', Password);
app.component('ProgressSpinner', ProgressSpinner);
app.component('Toast', Toast);
app.component('Card', Card);
app.component('InputText', InputText);
app.component('Breadcrumb', Breadcrumb);
app.component('Skeleton', Skeleton);
app.component('Stepper', Stepper);
app.component('StepList', StepList);
app.component('StepPanels', StepPanels);
app.component('StepItem', StepItem);
app.component('Step', Step);
app.component('StepPanel', StepPanel);
app.component('Checkbox', Checkbox);
app.component('Tag', Tag);
app.component('DataTable', DataTable);
app.component('Column', Column);
app.component('Divider', Divider);
app.component('Accordion', Accordion);
app.component('AccordionPanel', AccordionPanel);
app.component('AccordionHeader', AccordionHeader);
app.component('AccordionContent', AccordionContent);
app.component('Select', Select);
app.component('Dialog', Dialog);
app.component('InputNumber', InputNumber);
app.component('InputSwitch', InputSwitch);
app.component('MultiSelect', MultiSelect);
app.component('ConfirmDialog', ConfirmDialog);
app.component('Tabs', Tabs);
app.component('TabList', TabList);
app.component('Tab', Tab);
app.component('TabPanels', TabPanels);
app.component('TabPanel', TabPanel);
app.component('PickList', PickList);

app.component('AddUserDialog', AddUserDialog);
app.component('UserDetailDialogWrapper', UserDetailDialogWrapper);

app.directive('Tooltip', Tooltip);

app.component('Sidebar', Sidebar);
app.component('Topbar', Topbar);

app.mount('#app');
