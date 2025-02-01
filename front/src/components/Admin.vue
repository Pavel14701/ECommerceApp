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
  import AppHeader from '@/components/blocks/AppHeader.vue'
  import HeroCarousel from '@/components/blocks/HeroCarousel.vue'
  import HeroCarouselInverted from '@/components/blocks/HeroCarouselInverted.vue'
  import AdBanner from '@/components/blocks/AdBanner.vue'
  import AppFeatures from '@/components/blocks/AppFeatures.vue'
  import PromoCarousel from '@/components/blocks/PromoCarousel.vue'
  import ProductSlider from '@/components/blocks/ProductSlider.vue'
  import ProductCard from '@/components/blocks/ProductCard.vue'
  import AppFooter from '@/components/blocks/AppFooter.vue'
  
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
  