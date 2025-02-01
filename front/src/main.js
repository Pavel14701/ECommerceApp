import { createApp } from 'vue';
import App from './App.vue';
import vuetify from './plugins/vuetify';
import { loadFonts } from './plugins/webfontloader';
import router from './router'; // Импортируем маршрутизатор

loadFonts();

createApp(App)
  .use(vuetify)
  .use(router) // Используем маршрутизатор
  .mount('#app');
