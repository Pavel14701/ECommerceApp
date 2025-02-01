<template>
    <div class="block-wrapper" ref="carouselWrapper">
      <div class="carousel-wrapper">
        <div class="carousel-container">
          <v-carousel
            v-model="current"
            :show-arrows="false"
            :hide-delimiters="true"
            class="custom-carousel"
            continuous
            height="500"
          >
            <v-carousel-item v-for="(item, i) in items" :key="i" :transition="false">
              <v-img :src="item.src" class="carousel-image" contain />
            </v-carousel-item>
          </v-carousel>
          <div class="controls-description">
            <div class="description">
              <p>{{ items[current].description }}</p>
            </div>
            <div class="pagination">
              <v-btn icon @click="prevSlide">
                <v-icon>mdi-chevron-left</v-icon>
              </v-btn>
              <span>{{ current + 1 }} / {{ items.length }}</span>
              <v-btn icon @click="nextSlide">
                <v-icon>mdi-chevron-right</v-icon>
              </v-btn>
            </div>
          </div>
        </div>
      </div>
      <div class="additional-images">
        <div class="image-block" v-for="(image, i) in additionalItems" :key="i">
          <v-img :src="image.src" width="100%" contain />
          <p>{{ image.description }}</p>
        </div>
      </div>
    </div>
  </template>
  
  <script>
  export default {
    name: "HeroCarousel",
    data() {
      return {
        current: 0,
        items: [
          { src: require("@/assets/image1.webp"), description: "Описание картинки 1" },
          { src: require("@/assets/image1.webp"), description: "Описание картинки 2" }
        ],
        additionalItems: [
          { src: require("@/assets/image1.webp"), description: "Дополнительное описание 1" },
          { src: require("@/assets/image1.webp"), description: "Дополнительное описание 2" }
        ],
        observer: null,
        isVisible: false
      };
    },
    methods: {
      nextSlide() {
        if (this.isVisible) {
          this.current = (this.current + 1) % this.items.length;
        }
      },
      prevSlide() {
        if (this.isVisible) {
          this.current = (this.current - 1 + this.items.length) % this.items.length;
        }
      },
      startAutoplay() {
        this.autoplayInterval = setInterval(this.nextSlide, 3000); // Switch every 3 seconds
      },
      stopAutoplay() {
        clearInterval(this.autoplayInterval);
      },
      handleIntersection(entries) {
        entries.forEach((entry) => {
          this.isVisible = entry.isIntersecting;
        });
      },
      observeVisibility() {
        this.observer = new IntersectionObserver(this.handleIntersection);
        this.observer.observe(this.$refs.carouselWrapper);
      }
    },
    mounted() {
      this.observeVisibility();
      this.startAutoplay();
    },
    beforeUnmount() {
      this.stopAutoplay();
      if (this.observer) {
        this.observer.disconnect();
      }
    }
  };
  </script>
  
  <style scoped>
  .block-wrapper {
    margin-top: 3%;
    margin-bottom: 3%;
    display: flex;
    gap: 50px;
    justify-content: space-between;
    box-sizing: border-box;
    padding: 0 5%;
    align-items: flex-start;
  }
  
  .carousel-wrapper {
    flex: 1;
  }
  
  .carousel-container {
    width: 100%;
  }
  
  .carousel-image {
    width: 100%;
    object-fit: cover;
    height: 500px;
  }
  
  .custom-carousel .v-window-transition-enter-active,
  .custom-carousel .v-window-transition-leave-active {
    transition: opacity 0.5s ease;
  }
  
  .custom-carousel .v-window-transition-enter-from,
  .custom-carousel .v-window-transition-leave-to {
    opacity: 0;
  }
  
  .controls-description {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 5px;
    margin-left: 5%;
    margin-right: 5%;
  }
  
  .pagination {
    display: flex;
    align-items: center;
  }
  
  .pagination span {
    margin: 0 10px;
  }
  
  .description {
    text-align: left;
    flex-grow: 1;
  }
  
  .additional-images {
    width: 25%;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    gap: 85px;
  }
  
  .image-block {
    display: flex;
    flex-direction: column;
    justify-content: flex-start;
    align-items: center;
  }
  
  .image-block v-img {
    height: 500px;
    object-fit: cover;
    width: 100%;
  }
  
  .image-block p {
    text-align: center;
    margin-top: 10px;
  }
  </style>
  