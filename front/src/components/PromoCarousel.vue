<template>
  <v-carousel cycle height="300px">
    <v-carousel-item v-for="(item, i) in items" :key="i">
      <v-img :src="item.src" height="100%"></v-img>
    </v-carousel-item>
  </v-carousel>
</template>

<script>
export default {
  name: 'PromoCarousel',
  data() {
    return {
      items: [
        { src: 'https://via.placeholder.com/800x200?text=Promo+1' },
        { src: 'https://via.placeholder.com/800x200?text=Promo+2' },
      ]
    }
  }
}
</script>
