import { createRouter, createWebHistory } from 'vue-router';

const routes = [
    { path: '/', name: 'Home', component: () => import('../views/HomeView.vue') },
    { path: '/FaceRecognizer', name: 'FaceRecognizer', component: () => import('../views/FaceRecognizer.vue') },
    { path: '/FraudDetection', name: 'FraudDetection', component: () => import('../views/FraudDetection.vue') },
    { path: '/FormRecognizer', name: 'Page3', component: () => import('../views/FormRecognizer.vue') }
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;
