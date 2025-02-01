<template>
  <v-app>
    <AppHeader />
    <v-main>
      <router-view></router-view> <!-- Добавляем router-view -->
    </v-main>
    <AppFooter />
  </v-app>
</template>

<script>
export default {
  name: 'App',
};
</script>
