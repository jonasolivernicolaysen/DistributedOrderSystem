import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { toast } from "react-toastify"
import { apiFetch } from "../services/api"
interface Payment {
    paymentId: string;
    orderId: string;
    status: string;
    paidAt: string;
    userId: string;
    totalPrice: number;
    items: PaymentItem[];
}

interface PaymentItem {
    paymentItemId: string;
    paymentId: string;
    productId: string;
    productName: string;
    unitPrice: number;
    quantity: number;
}

function PaymentPage() {
    const [payment, setPayment] = useState<Payment | null>(null);
    const navigate = useNavigate();
    const { paymentId } = useParams();

    const pay = async (paymentProcessingId: string) => {
        try {
            const jwt = localStorage.getItem("jwt");
            const response = await apiFetch("https://localhost:7144/api/payments", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${jwt}`
                },
                body: JSON.stringify({ paymentId: paymentProcessingId })
            });

            if (!response.ok) {
                const error = await response.json();
                toast.error(error.error);
                return;
            }

            toast.success("Payment completed successfully");
            navigate("/products");
        } catch (error) {
            console.error(error);
            toast.error("Could not connect to the server");
        }
    };

    useEffect(() => {
        let cancelled = false;

        const getPaymentDetails = async (paymentId: string) => {
            const jwt = localStorage.getItem("jwt");

            while (!cancelled) {
                const response = await apiFetch(`https://localhost:7144/api/payments/${paymentId}`, {
                    method: "GET",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${jwt}`
                    }
                });

                if (response.ok) {
                    const data = await response.json();
                    if (!cancelled) setPayment(data);
                    break;
                }

                await new Promise(resolve => setTimeout(resolve, 1000));
            }
        };

        if (paymentId) getPaymentDetails(paymentId);

        return () => { cancelled = true; };
    }, [paymentId]);

    if (!payment) {
        return (
            <div className="container mt-4">
                <div className="text-muted small">Preparing payment...</div>
            </div>
        );
    }

    return (
        <div className="container mt-4">


            <div className="d-flex justify-content-between align-items-center mb-4">
                <h4 className="mb-0">Payment</h4>
                <span className="text-muted small">ID: {payment.paymentId}</span>
            </div>


            <div className="row fw-semibold text-muted small border-bottom pb-2 mb-2 px-2">
                <div className="col-5">Product</div>
                <div className="col-2 text-end">Price</div>
                <div className="col-2 text-center">Qty</div>
                <div className="col-3 text-end">Subtotal</div>
            </div>


            {payment.items.map((item: PaymentItem) => (
                <div key={item.paymentItemId} className="row align-items-center border-bottom py-3 px-2">
                    <div className="col-5 fw-semibold">{item.productName}</div>
                    <div className="col-2 text-end text-muted small">${item.unitPrice.toFixed(2)}</div>
                    <div className="col-2 text-center text-muted small">{item.quantity}</div>
                    <div className="col-3 text-end fw-semibold">${(item.unitPrice * item.quantity).toFixed(2)}</div>
                </div>
            ))}


            <div className="d-flex justify-content-between align-items-center mt-4">
                <span className="text-muted small">
                    {payment.items.length} item{payment.items.length !== 1 ? "s" : ""}
                </span>
                <div className="d-flex align-items-center gap-3">
                    <span className="fw-semibold">Total: ${payment.totalPrice.toFixed(2)}</span>
                    <button
                        className="btn btn-primary btn-sm"
                        onClick={() => pay(paymentId!)}
                    >
                        Pay now
                    </button>
                </div>
            </div>

        </div>
    );
}

export default PaymentPage;