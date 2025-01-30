<template>
  <v-app>
    <AppHeader />
    <v-main>
      <HeroCarousel />
      <ProductSlider />
      <AppFeatures />
      <HeroCarouselInverted />
      <ProductCard />
      <PromoCarousel />
      <AdBanner />
    </v-main>
    <AppFooter />
  </v-app>
</template>

<script>
import AppHeader from './components/AppHeader.vue'
import HeroCarousel from './components/HeroCarousel.vue'
import HeroCarouselInverted from './components/HeroCarouselInverted.vue'
import AdBanner from './components/AdBanner.vue'
import AppFeatures from './components/AppFeatures.vue'
import PromoCarousel from './components/PromoCarousel.vue'
import ProductSlider from './components/ProductSlider.vue'
import ProductCard from './components/ProductCard.vue'
import AppFooter from './components/AppFooter.vue'

export default {
  name: 'App',
  components: {
    AppHeader,
    HeroCarousel,
    HeroCarouselInverted,
    AppFeatures,
    PromoCarousel,
    ProductSlider,
    ProductCard,
    AdBanner,
    AppFooter
  }
}
</script>

<style>
body {
  margin: 0;
  font-family: 'Roboto', sans-serif;
}
</style>
