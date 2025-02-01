<template>
  <v-app-bar app color="primary" dark>
    <v-btn variant="text" @click="goToCategory('women')">Woman</v-btn>
    <v-btn variant="text" @click="goToCategory('men')">Men</v-btn>
    <v-btn variant="text" @click="goToCategory('kids')">Kids</v-btn>
    <v-spacer></v-spacer>
    <v-toolbar-title>Fashion Store</v-toolbar-title>
    <v-spacer></v-spacer>
    <v-text-field v-model="search" append-icon="mdi-magnify" label="Поиск" hide-details single-line></v-text-field>
    <v-btn icon @click="login">
      <v-icon>mdi-account</v-icon>
    </v-btn>
    <v-btn icon>
      <v-icon>mdi-cart</v-icon>
    </v-btn>
  </v-app-bar>
</template>

<script>
export default {
  name: 'AppHeader',
  data() {
    return {
      search: '',
    }
  },
  methods: {
    goToCategory() {
      // Логика перехода
    },
    login() {
      // Логика логина
    },
  }
}
</script>
