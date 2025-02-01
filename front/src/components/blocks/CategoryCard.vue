<template>
    <v-card class="image-card">
      <v-img :src="image" height="200px">
        <v-card-title class="overlay-title">{{ title }}</v-card-title>
      </v-img>
      <v-card-text>{{ description }}</v-card-text>
    </v-card>
  </template>
  
  <script>
  export default {
    name: 'CategoryCard',
    props: {
      title: {
        type: String,
        required: true,
      },
      image: {
        type: String,
        required: true,
      },
      description: {
        type: String,
        required: true,
      },
    },
  }
  </script>
  
  <style scoped>
  .image-card {
    position: relative;
    overflow: hidden;
  }
  
  .overlay-title {
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    color: white;
    background: rgba(0, 0, 0, 0.5);
  }
  </style>
  