<template>
  <v-container class="product-slider-wrapper">
    <v-row justify="center">
      <v-col cols="12">
        <div class="carousel-container">
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
          <!-- Левая стрелка управления -->
          <v-btn v-if="currentSlide > 0" icon class="control-left" @click="prevSlide">
            <v-icon>mdi-chevron-left</v-icon>
          </v-btn>
          <!-- Правая стрелка управления -->
          <v-btn v-if="currentSlide < groupedProducts.length - 1" icon class="control-right" @click="nextSlide">
            <v-icon>mdi-chevron-right</v-icon>
          </v-btn>
        </div>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
import ProductCard from './ProductCard.vue';

export default {
  name: 'ProductSlider',
  components: {
    ProductCard,
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
    groupedProducts() {
      const groups = [];
      for (let i = 0; i < this.products.length; i += 5) {
        groups.push(this.products.slice(i, i + 5));
      }
      return groups;
    },
  },
  methods: {
    nextSlide() {
      if (this.currentSlide < this.groupedProducts.length - 1) {
        this.currentSlide += 1;
      }
    },
    prevSlide() {
      if (this.currentSlide > 0) {
        this.currentSlide -= 1;
      }
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
  margin-right: 5%;
  margin-left: 5%;
  position: relative;
}

.carousel-container {
  position: relative;
}

.product-row {
  display: flex;
  flex-wrap: nowrap;
  justify-content: flex-start;
  align-items: center;
}

.product-card-col {
  flex: 0 0 20%;
  max-width: 20%;
  padding-left: 4px;
  padding-right: 4px;
}

.control-left,
.control-right {
  position: absolute;
  top: 40%;
  transform: translateY(-50%);
  display: flex;
  align-items: center;
  justify-content: center;
  width: 50px;
  height: 50px;
  background-color: rgba(0, 0, 0, 0.5);
  color: white;
  z-index: 100;
}

.control-left {
  left: -30px;
}

.control-right {
  right: -30px;
}

@media (max-width: 1200px) {
  .product-card-col {
    flex: 1 1 25%;
    max-width: 25%;
  }
}

@media (max-width: 992px) {
  .product-card-col {
    flex: 1 1 33.33%;
    max-width: 33.33%;
  }
}

@media (max-width: 768px) {
  .product-card-col {
    flex: 1 1 50%;
    max-width: 50%;
  }
}

@media (max-width: 576px) {
  .product-card-col {
    flex: 1 1 100%;
    max-width: 100%;
  }
}
</style>
