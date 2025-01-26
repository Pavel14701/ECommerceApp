<template>
  <div class="section">
    <div class="container">
      <h2 class="header">{{ title }}</h2>
      <p>{{ description }}</p>
    </div>
  </div>
</template>

<script>
export default {
  name: 'DescriptionBlock',
  props: {
    title: String,
    description: String
  }
}
</script>

<style scoped>
.section {
  padding: 2rem 0;
  background-color: #f9f9f9;
  border: 1px solid #ddd;
  margin: 1rem 0;
}
</style>
