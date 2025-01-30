<template>
  <v-container class="banner-container" fluid>
    <v-row align="center" justify="center" class="fill-height">
      <v-col cols="12">
        <v-carousel
          hide-delimiters
          hide-delimiters-background
          continuous
          cycle
          interval="3000"
          class="fade-carousel"
          :show-arrows="false"
        >
          <v-carousel-item v-for="(banner, index) in banners" :key="index">
            <v-card :style="{ backgroundColor: banner.color, backgroundImage: banner.image ? 'url(' + banner.image + ')' : '' }" flat class="banner">
              <v-card-title class="text-h3">{{ banner.title }}</v-card-title>
              <v-card-subtitle class="text-h5">скидки до <span class="discount">{{ banner.discount }}</span></v-card-subtitle>
              <v-card-actions class="advert-label">
                Реклама
              </v-card-actions>
            </v-card>
          </v-carousel-item>
        </v-carousel>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
export default {
  name: 'AdBanner',
  data() {
    return {
      banners: [
        { title: 'MANGO MAN', discount: '70%', color: '#ff6600', image: '' },
        { title: 'SUMMER SALE', discount: '50%', color: '#009688', image: '' },
        { title: 'WINTER COLLECTION', discount: '30%', color: '#3f51b5', image: '' },
        { title: 'SPRING SPECIAL', discount: '40%', color: '#e91e63', image: '' }
      ]
    };
  }
};
</script>

<style scoped>
.banner-container {
  padding: 0 5%;
  display: flex;
  width: 100%;
  height: 30vh;
  align-items: center;
  justify-content: center;
}

.banner {
  background-size: cover;
  background-position: center;
  color: white;
  width: 100%;
  position: relative;
  text-align: center;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.discount {
  font-weight: bold;
  font-size: 1.5em;
}

.advert-label {
  position: absolute;
  top: 10px;
  right: 10px;
  background-color: rgba(0, 0, 0, 0.24);
  color: white;
  padding: 2px 10px;
  font-size: 0.5em;
}

.fade-carousel .v-window-item {
  transition: opacity 1s ease;
  opacity: 0;
}

.fade-carousel .v-window-item--active {
  opacity: 1;
}
</style>
