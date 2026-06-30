import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { apiFetch } from "../services/api"
 
interface CartItem {
    productId: string;
    name: string;
    description: string;
    unitPrice: number;
    quantity: number;
}

function CartPage() {
    const [products, setProducts] = useState<CartItem[]>([]);
    const navigate = useNavigate();


    const checkout = async () => {
        try {
            const jwt = localStorage.getItem("jwt");
            const response = await apiFetch("http://localhost:7144/api/orders/cart/checkout", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${jwt}`
                }
            });

            if (!response.ok) {
                toast.error(`Request failed: ${response.status}`);
                return;
            }

            const data = await response.json();
            navigate(`/payments/${data.paymentId}`);
        } catch (error) {
            console.error(error);
            toast.error("Could not connect to the server");
        }
    };

    const totalPrice = products.reduce(
        (sum, item) => sum + item.unitPrice * item.quantity, 0
    );

    const getCartItems = async () => {
        try {
            const jwt = localStorage.getItem("jwt");
            const response = await apiFetch("http://localhost:7144/api/orders/cart", {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${jwt}`
                }
            });

            if (!response.ok) {
                toast.error(`Request failed: ${response.status}`);
                return;
            }

            const data = await response.json();
            setProducts(data.items);
        } catch (error) {
            console.error(error);
            toast.error("Could not connect to the server");
        }
    };

    const deleteItem = async (productId: string) => {
        const jwt = localStorage.getItem("jwt");
        const response = await apiFetch(`http://localhost:7144/api/orders/cart/${productId}`, {
            method: "DELETE",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${jwt}`
            }
        });

        if (!response.ok) {
            toast.error(`Request failed: ${response.status}`);
            return;
        }

        getCartItems();
    };

    useEffect(() => {
        getCartItems();
    }, []);

    return (
        <div className="container mt-4">

            <div className="d-flex justify-content-between align-items-center mb-4">
                <h4 className="mb-0">Cart</h4>
                <button
                    className="btn btn-outline-secondary btn-sm"
                    onClick={() => navigate("/products")}
                >
                    ← Back to Products
                </button>
            </div>

            {products.length === 0 && (
                <div className="text-center py-5 text-muted">
                    <p className="mb-2">Your cart is empty.</p>
                    <button
                        className="btn btn-primary btn-sm"
                        onClick={() => navigate("/products")}
                    >
                        Browse Products
                    </button>
                </div>
            )}

            {products.length > 0 && (
                <>
                    <div className="row fw-semibold text-muted small border-bottom pb-2 mb-2 px-2">
                        <div className="col-5">Product</div>
                        <div className="col-2 text-end">Price</div>
                        <div className="col-2 text-center">Qty</div>
                        <div className="col-3 text-end">Subtotal</div>
                    </div>

                    {products.map((item: CartItem) => (
                        <div key={item.productId} className="row align-items-center border-bottom py-3 px-2">
                            <div className="col-5">
                                <span className="fw-semibold">{item.name}</span>
                                <button
                                    className="btn btn-outline-danger btn-sm ms-2"
                                    onClick={() => deleteItem(item.productId)}
                                >
                                    Delete
                                </button>
                            </div>
                            <div className="col-2 text-end text-muted small">
                                ${item.unitPrice.toFixed(2)}
                            </div>
                            <div className="col-2 text-center text-muted small">
                                {item.quantity}
                            </div>
                            <div className="col-3 text-end fw-semibold">
                                ${(item.unitPrice * item.quantity).toFixed(2)}
                            </div>
                        </div>
                    ))}

                    
                    <div className="d-flex justify-content-between align-items-center mt-4">
                        <span className="text-muted small">
                            {products.length} item{products.length !== 1 ? "s" : ""}
                        </span>
                        <div className="d-flex align-items-center gap-3">
                            <span className="fw-semibold">Total: ${totalPrice.toFixed(2)}</span>
                            <button
                                className="btn btn-primary btn-sm"
                                onClick={checkout}
                            >
                                Checkout
                            </button>
                        </div>
                    </div>
                </>
            )}
        </div>
    );
}

export default CartPage;