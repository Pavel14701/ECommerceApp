<template>
  <div id="app">
    <AppHeader />
    <main>
      <HeroBlock />
      <DescriptionBlock title="About Us" description="We offer the latest trends in fashion." />
      <ProductSliderBlock />
      <DescriptionBlock title="Our Mission" description="To provide high-quality clothing at affordable prices." />
    </main>
    <AppFooter />
  </div>
</template>

<script>
import AppHeader from './components/Header.vue';
import HeroBlock from './components/Hero.vue';
import DescriptionBlock from './components/DescriptionBlock.vue';
import ProductSliderBlock from './components/ProductSlider.vue';
import AppFooter from './components/Footer.vue';

export default {
  name: 'App',
  components: {
    AppHeader,
    HeroBlock,
    DescriptionBlock,
    ProductSliderBlock,
    AppFooter
  }
}
</script>

<style>
body {
  font-family: Arial, sans-serif;
  margin: 0;
  padding: 0;
}
</style>
