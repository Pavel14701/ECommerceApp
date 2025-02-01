<template>
    <v-card
      class="product-card"
      @mousemove="handleMouseMove"
      @mouseleave="handleMouseLeave"
    >
      <div class="image-container">
        <v-img
          v-if="currentImage"
          class="product-image"
          :src="currentImage"
          height="300px"
          @load="onImageLoad"
          @error="onImageError"
        >
          <template #placeholder>
            <v-progress-circular
              indeterminate
              size="64"
              class="loading-spinner"
            ></v-progress-circular>
          </template>
        </v-img>
        <div
          v-else
          class="loading-placeholder"
        >
          <v-progress-circular
            indeterminate
            size="64"
            class="loading-spinner"
          ></v-progress-circular>
        </div>
        <div class="discount-badge" v-if="product && product.discount">-{{ product.discount }}%</div>
        <v-icon
          class="favorite-icon"
          :color="isFavorite ? 'red' : 'grey lighten-1'"
          @click.stop="toggleFavorite"
        >
          {{ isFavorite ? 'mdi-heart' : 'mdi-heart-outline' }}
        </v-icon>
      </div>
      <div class="image-counter" v-if="hovering && product && product.images">
        <span
          v-for="(image, index) in product.images"
          :key="index"
          :class="{ active: index === currentImageIndex }"
        ></span>
      </div>
      <v-card-title class="title small-text" style="white-space: pre-wrap">{{ product ? product.title : '' }}</v-card-title>
      <div class="price-container">
        <v-card-subtitle v-if="product && hasDiscount" class="price small-text">
          <s>{{ product ? product.originalPrice : '' }} ₽</s>
        </v-card-subtitle>
        <v-card-subtitle :class="{ 'discounted-price': hasDiscount, price: !hasDiscount }">
          {{ product ? finalPrice : '' }} ₽
        </v-card-subtitle>
      </div>
      <div class="rating-container small-text">
        <v-icon>mdi-star</v-icon> {{ product ? product.rating : '' }}
        <span>({{ product ? product.reviews : '' }} отзывов)</span>
      </div>
      <p class="sizes small-text" v-if="hovering && product">{{ product ? product.sizes.join(', ') : '' }}</p>
    </v-card>
  </template>
  
  <script>
  export default {
    name: 'ProductCard',
    props: {
      product: {
        type: Object,
        required: true
      }
    },
    data() {
      return {
        currentImage: '',
        currentImageIndex: 0,
        hovering: false,
        isFavorite: false, // Состояние избранного
        imageLoaded: false,
      };
    },
    computed: {
      hasDiscount() {
        return this.product && (this.product.discountedPrice || this.product.discount);
      },
      finalPrice() {
        if (this.product) {
          if (this.product.discountedPrice) {
            return this.product.discountedPrice;
          } else if (this.product.discount) {
            return (this.product.originalPrice - (this.product.originalPrice * this.product.discount) / 100).toFixed(2);
          } else {
            return this.product.originalPrice;
          }
        }
        return '';
      },
    },
    methods: {
      handleMouseMove(event) {
        if (this.product && this.product.images) {
          const cardWidth = event.currentTarget.offsetWidth;
          const mouseX = event.offsetX;
          const percentage = mouseX / cardWidth;
          const imageIndex = Math.floor(percentage * this.product.images.length);
          this.currentImage = this.product.images[imageIndex];
          this.currentImageIndex = imageIndex;
          this.hovering = true;
        }
      },
      handleMouseLeave() {
        if (this.product && this.product.images) {
          this.currentImage = this.product.images[0];
          this.currentImageIndex = 0;
          this.hovering = false;
        }
      },
      toggleFavorite() {
        this.isFavorite = !this.isFavorite;
        if (this.isFavorite) {
          this.addToFavorites();
        } else {
          this.removeFromFavorites();
        }
      },
      addToFavorites() {
        console.log(`Товар ${this.product ? this.product.title : ''} добавлен в избранное.`);
        // Здесь можно добавить логику для сохранения состояния избранного, например, через Vuex или локальное хранилище
      },
      removeFromFavorites() {
        console.log(`Товар ${this.product ? this.product.title : ''} удален из избранного.`);
        // Здесь можно добавить логику для удаления состояния избранного, например, через Vuex или локальное хранилище
      },
      onImageLoad() {
        this.imageLoaded = true;
      },
      onImageError() {
        this.imageLoaded = false;
      }
    },
    created() {
      if (this.product && this.product.images) {
        this.currentImage = this.product.images[0];
      }
    },
    mounted() {
      if (this.currentImage) {
        const img = new Image();
        img.src = this.currentImage;
        img.onload = () => {
          this.imageLoaded = true;
        };
        img.onerror = () => {
          this.imageLoaded = false;
        };
      }
    }
  };
  </script>
  
  <style scoped>
  .product-card {
    width: 200px;
    margin: 20px;
    position: relative;
    border-radius: 3%;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.199), 0 4px 8px rgba(0, 0, 0, 0.281);
  }
  
  .image-container {
    position: relative;
  }
  
  .product-image {
    margin-top: -15px;
    width: 100%;
  }
  
  .loading-placeholder {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 300px;
    width: 100%;
  }
  
  .loading-spinner {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
  }
  
  .discount-badge {
    position: absolute;
    bottom: 3px;
    left: 0;
    background-color: #ff0000;
    color: white;
    padding: 3px 5px;
    font-size: 0.6em;
    font-weight: 500;
  }
  
  /* Стили для кнопки избранного */
  .favorite-icon {
    position: absolute;
    top: 25px;
    right: 5px;
    font-size: 24px; /* Устанавливает размер иконки */
    cursor: pointer;
  }
  
  .image-counter {
    position: absolute;
    top: 0;
    width: 100%;
    display: flex;
    justify-content: space-between;
    padding: 0 5px;
  }
  
  .image-counter span {
    flex: 1;
    height: 6px;
    margin: 0 2px;
    background-color: rgba(255, 255, 255, 0.5);
  }
  
  .image-counter span.active {
    background-color: rgba(255, 255, 255, 0.9);
  }
  
  .small-text {
    padding-left: 5%;
    padding-right: 5%;
    padding-bottom: 3%;
    font-size: 0.66em;
  }
  
  .price-container {
    display: flex;
    align-items: center;
    gap: 10px;
  }
  
  .price {
    font-size: 0.8em;
  }
  
  .discounted-price {
    font-size: 0.8em;
    color: #ff0000;
  }
  
  .rating-container {
    display: flex;
    align-items: center;
  }
  
  .rating-container v-icon {
    font-size: 1em;
    color: #ffd700;
  }
  
  .rating-container span {
    margin-left: 5px;
  }
  
  .title {
    font-size: 0.75em;
    white-space: pre-wrap;
  }
  </style>
  