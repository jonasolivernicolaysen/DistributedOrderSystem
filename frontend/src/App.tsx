import { BrowserRouter, Routes, Route } from "react-router-dom";

import RegisterPage from "./pages/RegisterPage";
import LoginPage from "./pages/LoginPage";
import ProductsPage from "./pages/ProductsPage";
import CartPage from "./pages/CartPage";
import ProfilePage from "./pages/ProfilePage";
import CreateProductPage from "./pages/CreateProductPage";
import PaymentPage from "./pages/PaymentPage";
import { AuthProvider } from "./components/AuthContext";

import Navbar from "./components/Navbar";

function App() {

    return (
        <AuthProvider>
            <BrowserRouter>
            <Navbar />
                <Routes>
                    <Route path="/" element={<LoginPage />} />
                    <Route path="/register" element={<RegisterPage />} />
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/products" element={<ProductsPage />} />
                    <Route path="/cart" element={<CartPage />} />
                    <Route path="/profile" element={<ProfilePage/>} />
                    <Route path="/products/create" element={<CreateProductPage/>} />
                    <Route path="/payments/:paymentId" element={<PaymentPage />} />
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    )
}

export default App;
