<template>
  <v-container class="product-slider-wrapper">
    <v-row justify="center">
      <v-col cols="12">
        <v-carousel hide-delimiters v-model="currentSlide" :show-arrows="false">
          <!-- Объединяем продукты в группы по 5 -->
          <v-carousel-item v-for="(productGroup, i) in groupedProducts" :key="i">
            <v-row justify="flex-start" align="center" class="product-row">
              <v-col v-for="(product, j) in productGroup" :key="j" cols="12" sm="2" class="product-card-col">
                <product-card :product="product"></product-card>
              </v-col>
            </v-row>
          </v-carousel-item>
        </v-carousel>
        <!-- Кастомные элементы управления -->
        <v-row justify="center" class="mt-4">
          <v-btn v-for="(productGroup, i) in groupedProducts" :key="i" @click="currentSlide = i" :class="{ 'primary': currentSlide === i }">
            {{ i + 1 }}
          </v-btn>
        </v-row>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
import ProductCard from './ProductCard.vue'; // Импортируйте компонент ProductCard

export default {
  name: 'ProductSlider',
  components: {
    ProductCard, // Зарегистрируйте компонент ProductCard
  },
  data() {
    return {
      products: [
      { title: 'Mango Man\nHoodie NUMBERS', images: [require('@/assets/products/image1.webp'), require('@/assets/products/image2.webp'), require('@/assets/products/image3.webp')], originalPrice: 6999, discountedPrice: 3999, discount: 42, rating: 4.75, reviews: 123, sizes: [46, 48, 50, 54, 56, 58], },
        { title: 'Another Product', images: [require('@/assets/products/image1.webp'), require('@/assets/products/image2.webp'), require('@/assets/products/image3.webp')], originalPrice: 5999, discountedPrice: 4999, discount: 20, rating: 4.5, reviews: 89, sizes: [44, 46, 48, 50, 52], },
        { title: 'Another Product', images: [require('@/assets/products/image1.webp'), require('@/assets/products/image2.webp'), require('@/assets/products/image3.webp')], originalPrice: 5999, discountedPrice: 4999, discount: 20, rating: 4.5, reviews: 89, sizes: [44, 46, 48, 50, 52], },
        { title: 'Another Product', images: [require('@/assets/products/image1.webp'), require('@/assets/products/image2.webp'), require('@/assets/products/image3.webp')], originalPrice: 5999, discountedPrice: 4999, discount: 20, rating: 4.5, reviews: 89, sizes: [44, 46, 48, 50, 52], },
        { title: 'Another Product', images: [require('@/assets/products/image1.webp'), require('@/assets/products/image2.webp'), require('@/assets/products/image3.webp')], originalPrice: 5999, discountedPrice: 4999, discount: 20, rating: 4.5, reviews: 89, sizes: [44, 46, 48, 50, 52], },
        { title: 'Another Product', images: [require('@/assets/products/image1.webp'), require('@/assets/products/image2.webp'), require('@/assets/products/image3.webp')], originalPrice: 5999, discountedPrice: 4999, discount: 20, rating: 4.5, reviews: 89, sizes: [44, 46, 48, 50, 52], },
        { title: 'Another Product', images: [require('@/assets/products/image1.webp'), require('@/assets/products/image2.webp'), require('@/assets/products/image3.webp')], originalPrice: 5999, discountedPrice: 4999, discount: 20, rating: 4.5, reviews: 89, sizes: [44, 46, 48, 50, 52], },
        { title: 'Another Product', images: [require('@/assets/products/image1.webp'), require('@/assets/products/image2.webp'), require('@/assets/products/image3.webp')], originalPrice: 5999, discountedPrice: 4999, discount: 20, rating: 4.5, reviews: 89, sizes: [44, 46, 48, 50, 52], },
        { title: 'Another Product', images: [require('@/assets/products/image1.webp'), require('@/assets/products/image2.webp'), require('@/assets/products/image3.webp')], originalPrice: 5999, discountedPrice: 4999, discount: 20, rating: 4.5, reviews: 89, sizes: [44, 46, 48, 50, 52], },
      ],
      currentSlide: 0,
      imageLoaded: false,
    };
  },
  computed: {
    // Группировка продуктов по 5 в каждой группе
    groupedProducts() {
      const groups = [];
      for (let i = 0; i < this.products.length; i += 5) {
        groups.push(this.products.slice(i, i + 5));
      }
      return groups;
    },
  },
  created() {
    this.products.forEach(product => {
      product.images.forEach(src => {
        const img = new Image();
        img.src = src;
        img.onload = () => {
          this.imageLoaded = true;
        };
      });
    });
  },
};
</script>

<style scoped>
.product-slider-wrapper {
  margin-top: 70px;
  margin-right: 5%;
  margin-left: 5%;
}

.product-row {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-start;
}

.product-card-col {
  flex: 1;
  max-width: 20%;
}
</style>
