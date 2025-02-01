import { createRouter, createWebHistory } from 'vue-router';
import Home from '@/components/Home.vue';
import Man from '@/components/Man.vue';
import Women from '@/components/Woman.vue';
import Kids from '@/components/Kids.vue';

import News from '@/components/News.vue';
import Post from '@/components/Post.vue';

import Profile from '@/components/Profile.vue';
import Login from '@/components/Login.vue';

import Product from '@/components/Product.vue';
import ProductGallery from '@/components/ProductGallery.vue';
import Cart from '@/components/Cart.vue';

import Admin from '@/components/Admin.vue';


const routes = [
  { path: '/', component: Home },
  { path: '/man', component: Man },
  { path: '/woman', component: Women },
  { path: '/kids', component: Kids },

  { path: '/news', component: News },
  { path: '/post', component: Post },

  { path: '/profile', component: Profile },
  { path: '/login', component: Login },

  { path: '/product', component: Product },
  { path: '/products-gallery', component: ProductGallery },
  { path: '/cart', component: Cart },

  { path: '/admin', component: Admin },

];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
